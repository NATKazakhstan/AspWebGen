namespace Nat.SqlDbInitializer
{
    using System.Collections.Generic;

    using Nat.Tools.QueryGeneration;
    using Nat.Tools.Specific;
    using Nat.Web.Tools.Initialization;

    public class DbInitializer : IInitializer
    {
        static DbInitializer()
        {
            SpecificInstances.DbConstants = SqlDbConstants.Instance;
            SpecificInstances.DbFactory = SqlClientFactoryConnectionSpecified.Instance;
            SpecificInstances.DbTypeConverter = SqlDbTypeConverter.Instance;
            SpecificInstances.QueryCulture = new QueryCulture();
            SpecificInstances.QueryCulture.AliaseCultures = new List<ILocalizationQueryCulture>();
            var aliaseCultures = SpecificInstances.QueryCulture.AliaseCultures;
            var culture = new CorrectionAliaseExpressionCulture(@"\bnameRu\b");
            culture.CultureCorrectionExpressions.Add("kk-KZ", "nameKz");
            culture.CultureCorrectionExpressions.Add("ru-RU", "nameRu");
            aliaseCultures.Add(culture);
        }

        public void Initialize()
        {
        }
    }
}