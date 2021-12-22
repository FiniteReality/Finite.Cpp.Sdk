using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.ProjectSystem;

namespace Finite.Cpp.Sdk.VisualStudio.Configuration
{
    //[Export(typeof(IProjectConfigurationDimensionsProvider))]
    //[AppliesTo(ProjectCapabilities.ProjectConfigurationsDeclaredDimensions)]
    internal sealed partial class ProjectConfigurationDimensionProvider
        : IProjectConfigurationDimensionsProvider3
    {
        private readonly IProjectLockService _lockService;

        public ProjectConfigurationDimensionProvider(IProjectLockService lockService)
        {
            _lockService = lockService;
        }

        private async Task<Dimension> FindDimensionFromProjectOrDefaultAsync(UnconfiguredProject project, Dimension defaultValue)
        {
            using var lck = await _lockService.ReadLockAsync();
            var xml = await lck.GetProjectXmlAsync(project.FullPath);

            var property = xml.PropertyGroups
                .SelectMany(x => x.Properties)
                .FirstOrDefault(x => x.Name.Equals(defaultValue.MSBuildProperty, StringComparison.OrdinalIgnoreCase));

            return property switch
            {
                { } => new(property.Name, defaultValue.MSBuildProperty, ProjectCollection.Unescape(property.Value).Split(';')),
                _ => defaultValue
            };
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetBestGuessDefaultValuesForDimensionsAsync(UnconfiguredProject project)
        {
            return AsEnumerable(
                await FindDimensionFromProjectOrDefaultAsync(project, ConfigurationDimension),
                await FindDimensionFromProjectOrDefaultAsync(project, PlatformDimension)
            );

            static IEnumerable<KeyValuePair<string, string>> AsEnumerable(params Dimension[] values)
                => values.Select(x => new KeyValuePair<string, string>(x.Name, string.Join(";", x.Value)));
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetDefaultValuesForDimensionsAsync(UnconfiguredProject project)
        {
            return AsEnumerable(
                await FindDimensionFromProjectOrDefaultAsync(project, ConfigurationDimension),
                await FindDimensionFromProjectOrDefaultAsync(project, PlatformDimension)
            );

            static IEnumerable<KeyValuePair<string, string>> AsEnumerable(params Dimension[] values)
                => values.Select(x => new KeyValuePair<string, string>(x.Name, x.Value[0]));
        }

        public async Task<IEnumerable<KeyValuePair<string, IEnumerable<string>>>> GetProjectConfigurationDimensionsAsync(UnconfiguredProject project)
        {
            return AsEnumerable(
                await FindDimensionFromProjectOrDefaultAsync(project, ConfigurationDimension),
                await FindDimensionFromProjectOrDefaultAsync(project, PlatformDimension)
            );

            static IEnumerable<KeyValuePair<string, IEnumerable<string>>> AsEnumerable(params Dimension[] values)
                => values.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Name, x.Value));
        }

        public Task OnDimensionValueChangedAsync(ProjectConfigurationDimensionValueChangedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
