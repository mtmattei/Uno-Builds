using Uno.Extensions.Reactive;

namespace Orbital.Presentation;

public partial record SkillsModel(ISkillsService Skills)
{
    public IListFeed<SkillInfo> AllSkills => ListFeed.Async(Skills.GetSkillsAsync);

    public IFeed<string> InstalledCount => AllSkills
        .AsFeed()
        .Select(list => list.Count.ToString());

    public IFeed<string> ActiveCount => AllSkills
        .AsFeed()
        .Select(list => list.Count(s => s.IsActive).ToString());

    public IFeed<string> InvocationCount => AllSkills
        .AsFeed()
        .Select(list => list.Sum(s => s.Invocations).ToString("N0"));

    public IFeed<string> AvgAccuracy => AllSkills
        .AsFeed()
        .Select(list =>
        {
            var withInvocations = list.Where(s => s.Invocations > 0).ToList();
            return withInvocations.Count > 0
                ? withInvocations.Average(s => s.Accuracy).ToString("P0")
                : "N/A";
        });
}
