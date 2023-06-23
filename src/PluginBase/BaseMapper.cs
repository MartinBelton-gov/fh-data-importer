using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PluginBase;

public class BaseMapper
{
    protected readonly IOrganisationClientService _organisationClientService;
    protected Dictionary<string, OrganisationWithServicesDto> _dictOrganisations;
    protected Dictionary<string, TaxonomyDto> _dictTaxonomies;
    protected readonly string _adminAreaCode;
    private readonly OrganisationWithServicesDto _parentLA;
    private readonly string _key;
    public delegate void UpdateProgress(string name, string message);
    public UpdateProgress? UpdateProgressDelegate { get; set; }
    protected BaseMapper(IOrganisationClientService organisationClientService, string adminAreaCode, OrganisationWithServicesDto parentLA, string key) 
    { 
        _organisationClientService = organisationClientService;
        _adminAreaCode = adminAreaCode;
        _parentLA = parentLA;
        _key = key;
        _dictTaxonomies = new Dictionary<string, TaxonomyDto>();
        _dictOrganisations = new Dictionary<string, OrganisationWithServicesDto>();
    }
    protected async Task<List<string>> AddOrUpdateDirectoryService(bool newOrganisation, OrganisationWithServicesDto serviceDirectoryOrganisation, ServiceDto newService, string serviceReferenceId, List<string> errors)
    {
        if (newOrganisation)
        {
            //Create Organisation
            serviceDirectoryOrganisation.Services.Add(newService);

            try
            {
                long id = await _organisationClientService.CreateOrganisation(serviceDirectoryOrganisation);
                serviceDirectoryOrganisation.Id = id;
                _dictOrganisations.Add($"{serviceDirectoryOrganisation.AdminAreaCode}{serviceDirectoryOrganisation.Name}", serviceDirectoryOrganisation);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to Create Organisation with Service Id:{serviceReferenceId} {ex.Message}");
            }

        }
        else
        {

            OrganisationWithServicesDto organisationWithServicesDto = await _organisationClientService.GetOrganisationById(serviceDirectoryOrganisation.Id.ToString());
            if (organisationWithServicesDto.Services == null)
            {
                organisationWithServicesDto.Services = new List<ServiceDto>();
            }

            organisationWithServicesDto.Services = organisationWithServicesDto.Services.Where(x => x.Id != newService.Id).ToList();
            organisationWithServicesDto.Services.Add(newService);

            try
            {
                await _organisationClientService.UpdateOrganisation(organisationWithServicesDto);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to Update Organisation with Service Id:{serviceReferenceId} {ex.Message}");
            }
        }

        return errors;
    }

    protected async Task CreateOrganisationDictionary()
    {
        List<OrganisationDto> organisations = await _organisationClientService.GetListOrganisations();
        var localAuthority = organisations.FirstOrDefault(x => x.Name.Contains(_key));
        if (localAuthority == null)
        {
            var id = await _organisationClientService.CreateOrganisation(_parentLA);
            if (id > 0)
            {
                _parentLA.Id = id;
                _dictOrganisations[$"{_parentLA.AdminAreaCode}{_parentLA.Name}"] = _parentLA;
            }
            
        }

        foreach (var organisation in organisations)
        {
            OrganisationWithServicesDto organisationWithServicesDto = new OrganisationWithServicesDto
            {
                Id = organisation.Id,
                AssociatedOrganisationId = organisation.AssociatedOrganisationId,
                AdminAreaCode = organisation.AdminAreaCode,
                OrganisationType = organisation.OrganisationType,
                Name = organisation.Name,
                Description = organisation.Description.Truncate(496) ?? string.Empty,
                Logo = organisation.Logo,
                Uri = organisation.Uri,
                Url = organisation.Url,
                Services = new List<ServiceDto>()
            };

            _dictOrganisations[$"{organisationWithServicesDto.AdminAreaCode}{organisationWithServicesDto.Name}"] = organisationWithServicesDto;

        }
    }

    protected async Task CreateTaxonomyDictionary()
    {
        _dictTaxonomies = new Dictionary<string, TaxonomyDto>();
        var allTaxonomies = await _organisationClientService.GetTaxonomyList(1, 9999);
        foreach (var taxonomy in allTaxonomies.Items)
        {
            _dictTaxonomies[taxonomy.Name.ToLower()] = taxonomy;
        }
    }

    protected void ProgressUpdate(string name, string message) 
    {
        Console.WriteLine(message);
        if (UpdateProgressDelegate != null)
        {
            UpdateProgressDelegate(name, message);
        }
    }
}
