using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Courier.Core;
using Umbraco.Courier.Core.Helpers;

namespace Id.PowershellExtensions.UmbracoResources
{
    public class ResourcePublisher
    {
        private readonly RepositoryProvider publicationTarget;
        private readonly IPsCmdletLogger logger;
        
        public ResourcePublisher(RepositoryProvider publicationTarget, IPsCmdletLogger logger, string pluginFolder)
        {
            this.publicationTarget = publicationTarget;
            this.logger = logger;

            logger.Log("Loading plugins from " + pluginFolder);

            Assembly.LoadFrom(Path.Combine(pluginFolder, "umbraco.courier.providers.dll"));
            Assembly.LoadFrom(Path.Combine(pluginFolder, "umbraco.courier.dataresolvers.dll"));

            Context.Current.BaseDirectory = Directory.GetCurrentDirectory();
            Context.Current.HasHttpContext = false;

            logger.Log("Current directory set to " + Directory.GetCurrentDirectory());
        }

        public void Publish(string revisionFolder)
        {
            Deploy(revisionFolder);
        }

        private void Deploy(string revisionFolder)
        {
            try
            {
                logger.Log("Loading extraction instance");

                var extractionManager = ExtractionManager.Instance;

                extractionManager.ExtractedItem += ManagerExtractedItem;
                extractionManager.Extracted += ManagerExtracted;

                extractionManager.OverwriteExistingDependencies = true;
                extractionManager.OverwriteExistingitems = true;
                extractionManager.OverwriteExistingResources = true;

                logger.Log("Deployment manager loaded");

                var revFolder = Path.GetFullPath(revisionFolder);
                logger.Log("Loading folder: " + revFolder);

                var repository = new Repository(publicationTarget);

                logger.Log("Enabling remote deployment for: " + repository.Name);
                extractionManager.EnableRemoteExtraction(repository);
                
                logger.Log("Loading Contents: " + revFolder);
                extractionManager.Load(revFolder);

                logger.Log("Building graph...");
                extractionManager.BuildGraph();

                logger.Log(extractionManager.ExtractionGraph.CountUnique() + " Items added to graph");

                logger.Log("Extraction...");
                extractionManager.ExtractAll(false, true);

                logger.Log("Unloading...");
                extractionManager.Unload();

                logger.Log("DONE...");

            }
            catch(Exception ex)
            {
                logger.Log(ex);
            }
            finally
            {
                ExtractionManager.Instance.ExtractedItem -= ManagerExtractedItem;
                ExtractionManager.Instance.Extracted -= ManagerExtracted;
            }
        }

        private void ManagerExtracted(object sender, ExtractionEventArgs e)
        {
            logger.Log("extraction completed");
        }

        private void ManagerExtractedItem(object sender, ItemEventArgs e)
        {
            logger.Log(e.Item.Name + " Extracted");
        }
    }
}
