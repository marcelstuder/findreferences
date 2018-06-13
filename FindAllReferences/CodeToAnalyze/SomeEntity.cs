namespace FindAllReferences.CodeToAnalyze
{
    public class SomeEntity 
    {
        public virtual string PropertyNotMapped { get; set; }
        public virtual string PropertyMappedButNotReferenced { get; set; }
        public virtual string PropertyMappedAndReferenced { get; set; }
    }
}
