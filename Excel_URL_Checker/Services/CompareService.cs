using Excel_URL_Checker.DTOs;
using Excel_URL_Checker.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Linq;

namespace Excel_URL_Checker.Services
{
    public class CompareService : ICompareService
    {
        public async Task<List<ChildrenDTO>> CompareData(List<ExcelDTO> Data, int Similarity, int KeysComparePercentage)
        {
            if (Data.Count > 0)
            {

                var result = await Task.Run(() =>
                {
                    return Data.AsParallel().Select(i =>
                   {
                       var keysGroup = new List<string>();

                       keysGroup.Add(i.Key);

                       var similarities = Data.AsParallel().Where(x =>
                       {
                           if (x.Key == i.Key) return false;
                           var DataOrganic = x.organic.Split(",").ToList();
                           var objOrganic = i.organic.Split(",").ToList();
                           var similaritesPercentage = ((100 / objOrganic.Count()) * DataOrganic.Intersect(objOrganic).Count());
                           return similaritesPercentage >= Similarity;

                       }).Select(c =>
                       {
                           keysGroup.Add(c.Key);
                           return new ChildrenDTO()
                           {

                               ID = Guid.NewGuid(),
                               Key = c.Key,
                               Difficulty = c.Difficulty,
                               SearchRate = c.SearchRate,
                               organic = c.organic,
                               ParentSearchLinkLength = i.organic.Split(",").Length,
                               SimilarityLinks = c.organic.Split(",").ToList().Intersect(i.organic.Split(",").ToList()).ToList(),
                               Similarity = ((100 / i.organic.Split(",").ToList().Count()) * c.organic.Split(",").ToList().Intersect(i.organic.Split(",").ToList()).Count())
                           };
                       }).ToList();
                       keysGroup = keysGroup.OrderBy(x => x).ToList();

                       var ChildrensMaxDifficulty = similarities.Count() > 0 ? similarities.Select(i => i).Max(i => i.Difficulty) : int.Parse("0");

                       return new ChildrenDTO()
                       {
                           ID = Guid.NewGuid(),
                           GroupKeys = keysGroup,
                           Key = i.Key,
                           Difficulty = i.Difficulty != null ? Math.Max(ChildrensMaxDifficulty ?? default(int), i?.Difficulty ?? default(int)) : ChildrensMaxDifficulty,
                           SearchRate = similarities.Select(i => i).Sum(i => i.SearchRate) + i.SearchRate,
                           organic = i.organic,
                           SimilarityChildrens = similarities,
                       };
                   }).OrderByDescending(x => x?.SimilarityChildrens?.Count).ToList();
                });
                var createNewMergedData = new List<ChildrenDTO>();
                foreach (var item in result.ToList())
                {
                    var foundedGroups = result.Where(i =>
                    {
                        var DataOrganic = i.GroupKeys;
                        var objOrganic = item.GroupKeys;

                        var similaritiesPercentage = ((100.0 / DataOrganic.Count()) * DataOrganic.Intersect(objOrganic).Count());
                        if (objOrganic.Count > DataOrganic.Count)
                        {
                            similaritiesPercentage = ((100.0 / objOrganic.Count()) * objOrganic.Intersect(DataOrganic).Count());
                        }
                        return similaritiesPercentage >= KeysComparePercentage;
                    }).ToList();

                    if (foundedGroups.Count > 0)
                    {
                        foundedGroups.Add(item);
                        var newMerged = new ChildrenDTO();
                        newMerged.ID = Guid.NewGuid();
                        newMerged.Key = item.Key;
                        newMerged.GroupKeys = foundedGroups.SelectMany(i => i.GroupKeys).Distinct().ToList();
                        newMerged.SimilarityChildrens = foundedGroups.SelectMany(i => i.SimilarityChildrens).DistinctBy(i=>i.Key).ToList();
                        newMerged.SearchRate = foundedGroups.DistinctBy(i => i.Key).Sum(i => i.SearchRate);
                        newMerged.Difficulty = foundedGroups.DistinctBy(i => i.Key).Max(i => i.Difficulty);
                        createNewMergedData.Add(newMerged);
                        result.RemoveAll(foundedGroups.Contains);
                    }
                }

                result.AddRange(createNewMergedData);


                //var newResult = new List<ChildrenDTO>();
                //foreach (var item in result.ToList())
                //{
                //    var foundedSimilarKeys = result.FindAll(i =>
                //    {
                //        if ((i.Key != item.Key) && (i.GroupKeys.Count() <= item.GroupKeys.Count()))
                //        {

                //            var itemsEqualsCount = item.GroupKeys.Intersect(i.GroupKeys).ToList().Count;
                //            var percentage = ((100 / item.GroupKeys.Count) * itemsEqualsCount);
                //            if (percentage >= KeysComparePercentage)
                //            {
                //                return true;
                //            }
                //        }
                //        return false;
                //    });

                //    var newChildrens = new List<ChildrenDTO>();

                //    var newParents = item;

                //    foreach (var child in item.SimilarityChildrens)
                //    {
                //        newChildrens.Add(child);
                //        foreach (var x in foundedSimilarKeys.ToList())
                //        {
                //            var newChilds = x.SimilarityChildrens.FindAll(i => i.Key != child.Key).ToList();
                //            foreach (var z in newChilds)
                //            {
                //                newChildrens.Add(z);
                //                foundedSimilarKeys.Remove(z);

                //            }

                //        }
                //    }
                //    var ChildrensMaxDifficulty = newChildrens.Count() > 0 ? newChildrens.Select(i => i).Max(i => i.Difficulty) : int.Parse("0");
                //    //Difficulty = i.Difficulty != null ? Math.Max(ChildrensMaxDifficulty ?? default(int), i?.Difficulty ?? default(int)) : ChildrensMaxDifficulty,
                //    //       SearchRate = similarities.Select(i => i).Sum(i => i.SearchRate) + i.SearchRate,
                //    newParents.SimilarityChildrens = newChildrens;
                //    newParents.Difficulty = item.Difficulty != null ? Math.Max(ChildrensMaxDifficulty ?? default(int), item?.Difficulty ?? default(int)) : ChildrensMaxDifficulty;
                //    newParents.SearchRate = newChildrens.Select(i => i).Sum(i => i.SearchRate) + item.SearchRate;
                //}






                return createNewMergedData;
            }

            return new List<ChildrenDTO>();

        }

    }
}
public class ListStringEqualityComparer : IEqualityComparer<List<string>>
{
    private readonly int _KeysComparePercentage;
    public ListStringEqualityComparer(int KeysComparePercentage)
    {
        _KeysComparePercentage = KeysComparePercentage;
    }
    public bool Equals(List<string> x, List<string> y)
    {

        var itemsEqualsCount = x.Intersect(y).ToList().Count;
        var percentage = ((100 / x.Count) * itemsEqualsCount);
        if (percentage >= _KeysComparePercentage)
        {
            return true;
        }
        return false;
    }

    public int GetHashCode(List<string> obj)
    {

        return obj.GetHashCode();
    }
    //public bool Equals(List<string> x, List<string> y)
    //{
    //    if (ReferenceEquals(x, y))
    //        return true;

    //    if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
    //        return false;

    //    return x.SequenceEqual(y);
    //}

    //public int GetHashCode(List<string> obj)
    //{
    //    unchecked
    //    {
    //        int hash = 17;

    //        foreach (var item in obj)
    //        {
    //            hash = hash * 23 + item.GetHashCode();
    //        }

    //        return hash;
    //    }
    //}
}
public class ChildrenDTO : ExcelDTO
{
    public Guid ID { get; set; }
    public List<string> GroupKeys { get; set; }
    public List<ChildrenDTO>? SimilarityChildrens { get; set; }
    public int ParentSearchLinkLength { get; set; }
    public int Similarity { get; set; }
    public List<string> SimilarityLinks { get; set; }
    public List<string> OrganicList
    {
        get
        {
            if (string.IsNullOrEmpty(this.organic)) return new List<string>();
            return this.organic.Split(',').ToList();
        }

    }
    public int SearchLinkLength
    {
        get
        {
            return OrganicList.Count;
        }

    }
}

public class GeneratedSimilarity
{
    public List<ChildrenDTO> Childrens { get; set; }
    public int? SearchRate { get; set; }
    public int? Difficulty { get; set; }

}