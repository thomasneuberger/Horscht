var idPrefix = '/providers/Microsoft.Authorization/roleDefinitions/'

@description('Read, write, and delete Azure Storage containers and blobs.')
output StorageBlobDataContributor string = '${idPrefix}ba92f5b4-2d11-453d-a403-e96b0029c9fe'

output StorageQueueDataContributor string = '${idPrefix}974c5e8b-45b9-4653-ba55-5f855dd0fb88'