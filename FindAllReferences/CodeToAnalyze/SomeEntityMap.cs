using FluentNHibernate.Mapping;

namespace FindAllReferences.CodeToAnalyze
{
    public class SomeEntityMap : ClassMap<SomeEntity>
    {
        public SomeEntityMap()
        {
            Map(x => x.PropertyMappedButNotReferenced).Nullable();
            Map(x => x.PropertyMappedAndReferenced).Not.Nullable();
        }
    }
}
