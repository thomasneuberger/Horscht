var idPrefix = '/providers/Microsoft.Authorization/roleDefinitions/'

@description('Read, write, and delete Azure Storage containers and blobs.')
output StorageBlobDataContributor string = '${idPrefix}ba92f5b4-2d11-453d-a403-e96b0029c9fe'