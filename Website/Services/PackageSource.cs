﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NuGetGallery
{
    public class PackageSource : IPackageSource
    {
        private readonly IEntityRepository<Package> _packageRepository;
        private readonly IEntityRepository<CuratedPackage> _curatedPackageRepository;
        private readonly IEntityRepository<PackageFavorite> _favoritesRepository;

        public PackageSource(EntitiesContext entitiesContext)
        {
            _packageRepository = new EntityRepository<Package>(entitiesContext);
            _curatedPackageRepository = new EntityRepository<CuratedPackage>(entitiesContext);
            _favoritesRepository = new EntityRepository<PackageFavorite>(entitiesContext);
        }

        [Ninject.Inject]
        public PackageSource(
            IEntityRepository<Package> packageRepo,
            IEntityRepository<CuratedPackage> curatedPackageRepo,
            IEntityRepository<PackageFavorite> favoritesRepo)
        {
            _packageRepository = packageRepo;
            _curatedPackageRepository = curatedPackageRepo;
            _favoritesRepository = favoritesRepo;
        }

        public IQueryable<PackageIndexEntity> GetPackagesForIndexing(DateTime? newerThan)
        {
            IQueryable<Package> set = _packageRepository.GetAll()
                .Where(p => p.IsLatest || p.IsLatestStable)  // which implies that p.IsListed by the way!
                .Include(p => p.PackageRegistration)
                .Include(p => p.PackageRegistration.Owners)
                .Include(p => p.SupportedFrameworks);

            if (newerThan.HasValue)
            {
                // Retrieve the Latest and LatestStable version of packages if any package for that registration changed since we last updated the index.
                // We need to do this because some attributes that we index such as DownloadCount are values in the PackageRegistration table that may
                // update independent of the package.
                set = set.Where(p => p.PackageRegistration.Packages.Any(p2 => p2.LastUpdated > newerThan));
            }

            var list1 = set.ToList();

            // Find all favorite relationships that have been updated
            // Build a representative package set corresponding to the list
            var updatedFavoritePackages = _favoritesRepository.GetAll()
                .Where(f => f.LastModified >= newerThan)
                .GroupBy(f => f.PackageRegistrationKey)
                .ToDictionary(group => group.Key);

            var updatedPackageRegistrationKeys = updatedFavoritePackages.Keys.ToArray();

            var updatedPackageRepresentatives = _packageRepository.GetAll()
                .Where(p => p.IsLatest || p.IsLatestStable)
                .Where(p => updatedPackageRegistrationKeys.Contains(p.PackageRegistrationKey))
                .Include(p => p.PackageRegistration)
                .Include(p => p.PackageRegistration.Owners)
                .Include(p => p.SupportedFrameworks);

            var list2 = updatedPackageRepresentatives.ToList();

            // Merge the lists of packages to reindex
            Dictionary<int, Package> finalSet = new Dictionary<int,Package>();
            foreach (var p in list1.Concat(list2))
            {
                finalSet[p.PackageRegistrationKey] = p;
            }

            var list = finalSet.Values;

            // Look up which curatedFeeds and which favorites refer to which package, 
            // and attach that information to the package for indexing
            var curatedFeedsPerPackageRegistration = _curatedPackageRepository.GetAll()
                .Select(cp => new { cp.PackageRegistrationKey, cp.CuratedFeedKey })
                .GroupBy(x => x.PackageRegistrationKey)
                .ToDictionary(group => group.Key, element => element.Select(x => x.CuratedFeedKey));

            var favoritersPerPackageRegistration = _favoritesRepository.GetAll()
                .Where(fav => fav.IsFavorited)
                .Select(fav => new { fav.PackageRegistrationKey, fav.User.Username })
                .GroupBy(x => x.PackageRegistrationKey)
                .ToDictionary(group => group.Key, element => element.Select(x => x.Username));

            var entities = list.Select(
                p => new PackageIndexEntity 
                {
                    Package = p, 
                    CuratedFeedKeys = curatedFeedsPerPackageRegistration.GetValueOrDefault(p.PackageRegistrationKey),
                    Favoriters = favoritersPerPackageRegistration.GetValueOrDefault(p.PackageRegistrationKey),
                });

            return entities.AsQueryable();
        }
    }
}
