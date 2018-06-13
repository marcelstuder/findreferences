namespace FindAllReferences.CodeToAnalyze
{
    public class SomeEntityRepo
    {
        public string GetSomeEventDetails()
        {
            var entity = new SomeEntity();
            return entity.PropertyMappedAndReferenced;
        }
    }
}
