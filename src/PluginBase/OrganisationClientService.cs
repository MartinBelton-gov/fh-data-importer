using RestSharp;
using FamilyHubs.SharedKernel;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace PluginBase
{
    public interface IOrganisationClientService
    {
        Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.ServiceCategory);
        Task<long> CreateTaxonomy(TaxonomyDto taxonomy);
        Task<List<OrganisationDto>> GetListOrganisations();
        Task<OrganisationWithServicesDto> GetOrganisationById(string id);
        Task<long> CreateOrganisation(OrganisationWithServicesDto organisation);
        Task<long> UpdateOrganisation(OrganisationWithServicesDto organisation);
    }

    public class OrganisationClientService : IOrganisationClientService
    {
        private readonly RestClient _client;

        public OrganisationClientService(string baseUri)
        {
            _client = new RestClient(baseUri);
        }

        public async Task<List<OrganisationDto>> GetListOrganisations()
        {
            var request = new RestRequest("api/organisations");

            return await _client.GetAsync<List<OrganisationDto>>(request, CancellationToken.None) ?? new List<OrganisationDto>();
        }

        public async Task<OrganisationWithServicesDto> GetOrganisationById(string id)
        {
            var request = new RestRequest($"api/organisations/{id}");

            return await _client.GetAsync<OrganisationWithServicesDto>(request, CancellationToken.None) ?? new OrganisationWithServicesDto
            { 
                AdminAreaCode = default!,
                OrganisationType = default!,
                Name = default!,
                AssociatedOrganisationId = default!,
                Description = default! 
            };
        }

        public async Task<long> CreateOrganisation(OrganisationWithServicesDto organisation)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(organisation);
            var request = new RestRequest($"api/organisations").AddJsonBody(json);
            request.RequestFormat = DataFormat.Json;

            RestResponse response = await _client.PostAsync(request);

            if (long.TryParse(response.Content, out long organisationId))
            {
                return organisationId;
            }

            return organisation.Id;
        }

        public async Task<long> UpdateOrganisation(OrganisationWithServicesDto organisation)
        {
            var request = new RestRequest($"api/organisations/{organisation.Id}").AddJsonBody(Newtonsoft.Json.JsonConvert.SerializeObject(organisation));
            request.RequestFormat = DataFormat.Json;

            RestResponse response = await _client.PutAsync(request);

            if (long.TryParse(response.Content, out long organisationId))
            {
                return organisationId;
            }

            return organisation.Id;
        }

        public async Task<PaginatedList<TaxonomyDto>> GetTaxonomyList(int pageNumber = 1, int pageSize = 10, TaxonomyType taxonomyType = TaxonomyType.ServiceCategory)
        {
            var request = new RestRequest($"api/taxonomies?pageNumber={pageNumber}&pageSize={pageSize}&taxonomyType={taxonomyType}");
            return await _client.GetAsync<PaginatedList<TaxonomyDto>>(request, CancellationToken.None) ?? new PaginatedList<TaxonomyDto>();
        }

        public async Task<long> CreateTaxonomy(TaxonomyDto taxonomy)
        {
            var request = new RestRequest($"api/taxonomies").AddJsonBody(Newtonsoft.Json.JsonConvert.SerializeObject(taxonomy));
            request.RequestFormat = DataFormat.Json;

            RestResponse response = await _client.PostAsync(request);

            if (long.TryParse(response.Content, out long taxonomyId))
            {
                return taxonomyId;
            }

            return 0;
        }
    }
}
