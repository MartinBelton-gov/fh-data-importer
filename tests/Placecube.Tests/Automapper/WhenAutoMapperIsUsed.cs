using AutoMapper;
using FamilyHubs.ServiceDirectory.Shared.Dto;
using FamilyHubs.ServiceDirectory.Shared.Enums;
using FluentAssertions;
using PlacecubeImporter;
using PlacecubeImporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Placecube.Tests.Automapper;

public class WhenAutoMapperIsUsed
{
    private readonly IMapper _mapper;
    public WhenAutoMapperIsUsed()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMappingProfiles>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void ThenCostOptionsMapsToCostOptionDto()
    {
        //Arrange
        CostOptions costOption = new CostOptions
        {
            id = "111",
            service_id = "222",
            valid_from = "2023-04-01",
            valid_to = "2023-04-02",
            option = "option",
            amount = 1.5M,
            amount_description = "amount description",
        };

        CostOptionDto costOptionDto = new CostOptionDto
        {
            Id = 111,
            ServiceId = 222,
            ValidFrom = new DateTime(2023,4,1),
            ValidTo = new DateTime(2023, 4, 2),
            Option = "option",
            Amount = 1.5M,
            AmountDescription = "amount description",
        };

        //Act
        var mappedCostOptionDto = _mapper.Map<CostOptionDto>(costOption);

        //Assert
        mappedCostOptionDto.Should().BeEquivalentTo(costOptionDto);
    }

    [Theory]
    [InlineData(null, EligibilityType.NotSet)]
    [InlineData("", EligibilityType.NotSet)]
    [InlineData("Adult", EligibilityType.Adult)]
    [InlineData("Child", EligibilityType.Child)]
    public void ThenEligibilityMapsToEligibilityDto(string eligibility, EligibilityType eligibilityType)
    {
        //Arrange
        Eligibility eligibilityItem = new Eligibility()
        {
            id = "111",
            service_id = "222",
            eligibility = eligibility,
            minimum_age = 2,
            maximum_age = 5
        };

        EligibilityDto eligibilityDto = new EligibilityDto
        { 
            Id = 111,
            ServiceId = 222,
            EligibilityType = eligibilityType,
            MinimumAge = 2,
            MaximumAge = 5
        };

        //Act
        var mappedEligibility = _mapper.Map<EligibilityDto>(eligibilityItem);

        //Assert
        mappedEligibility.Should().BeEquivalentTo(eligibilityDto);
    }

    [Fact]
    public void ThenFundingMapsToFundingDto()
    {
        //Arrange
        Funding funding = new Funding
        {
            id = "111",
            service_id = "222",
            source = "source"
        };

        FundingDto fundingDto = new FundingDto
        {
            Id = 111,
            ServiceId = 222,
            Source = "source"
        };

        //Act
        var mappedFundingDto = _mapper.Map<FundingDto>(funding);

        //Assert
        mappedFundingDto.Should().BeEquivalentTo(fundingDto);
    }

    [Fact]
    public void ThenLanguageMapsToLanguageDto()
    {
        //Arrange
        Language funding = new Language
        {
            id = "111",
            service_id = "222",
            language = "language",
        };

        LanguageDto languageDto = new LanguageDto
        {
            Id = 111,
            ServiceId = 222,
            Name = "language",
        };

        //Act
        var mappedLanguageDto = _mapper.Map<LanguageDto>(funding);

        //Assert
        mappedLanguageDto.Should().BeEquivalentTo(languageDto);
    }

    [Fact]
    public void ThenServiceAreaMapsToServiceAreaDto()
    {
        //Arrange
        ServiceArea serviceArea = new ServiceArea
        {
            id = "111",
            extent = "extent",
            service_area = "Service Area"

        };

        ServiceAreaDto serviceAreaDto = new ServiceAreaDto
        {
            Id = 111,
            Extent = "extent",
            ServiceAreaName = "Service Area"
        };

        //Act
        var mappedServiceAreaDto = _mapper.Map<ServiceAreaDto>(serviceArea);

        //Assert
        mappedServiceAreaDto.Should().BeEquivalentTo(serviceAreaDto);
    }

    [Fact]
    public void ThenLocationMapsToLocationDtoWithoutAddress()
    {
        List<AccessibilityForDisabilities> accessibilityForDisabilities = new List<AccessibilityForDisabilities>()
        {
            new AccessibilityForDisabilities 
            {
                accessibility = "accessibility"
            }
        };

        List<AccessibilityForDisabilitiesDto> accessibilityForDisabilitiesDtos = new List<AccessibilityForDisabilitiesDto>()
        {
            new AccessibilityForDisabilitiesDto
            {
                Accessibility = "accessibility" 
            }
        };

        Location location = new Location
        {
            id = "111",
            accessibility_for_disabilities = accessibilityForDisabilities.ToArray(),
            physical_addresses = default!,
            latitude = 1.123F,
            longitude = 2.234F,
            name = "name",
        };

        LocationDto locationDto = new LocationDto
        {
            Id = 111,
            LocationType = LocationType.FamilyHub,
            AccessibilityForDisabilities = accessibilityForDisabilitiesDtos.ToArray(),
            Latitude = 1.123F,
            Longitude = 2.234F,
            Name = "name",
            Address1 = default!, Address2 = default!,   
            City = default!,
            PostCode = default!,
            Country = default!,
            StateProvince = default!,
            HolidaySchedules = new List<HolidayScheduleDto>(),
            RegularSchedules = new List<RegularScheduleDto>(),

        };

        //Act
        var mappedLocationDto = _mapper.Map<LocationDto>(location);

        //Assert
        mappedLocationDto.Should().BeEquivalentTo(locationDto);
    }

    [Fact]
    public void ThenLocationMapsToLocationDtoWithAddress()
    {
        List<AccessibilityForDisabilities> accessibilityForDisabilities = new List<AccessibilityForDisabilities>()
        {
            new AccessibilityForDisabilities
            {
                accessibility = "accessibility"
            }
        };

        List<AccessibilityForDisabilitiesDto> accessibilityForDisabilitiesDtos = new List<AccessibilityForDisabilitiesDto>()
        {
            new AccessibilityForDisabilitiesDto
            {
                Accessibility = "accessibility"
            }
        };

        List<PhysicalAddresses> physicalAddresses = new List<PhysicalAddresses>()
        {
            new PhysicalAddresses
            {
                address_1 = "Address 1",
                city = "City",
                postal_code = "Post Code",
                country = "country",
                state_province = "State Province"
            }
        };

        Location location = new Location
        {
            id = "111",
            accessibility_for_disabilities = accessibilityForDisabilities.ToArray(),
            physical_addresses = physicalAddresses.ToArray(),
            latitude = 1.123F,
            longitude = 2.234F,
            name = "name",
        };

        LocationDto locationDto = new LocationDto
        {
            Id = 111,
            LocationType = LocationType.FamilyHub,
            AccessibilityForDisabilities = accessibilityForDisabilitiesDtos.ToArray(),
            Latitude = 1.123F,
            Longitude = 2.234F,
            Name = "name",
            Address1 = "Address 1",
            Address2 = default!,
            City = "City",
            PostCode = "Post Code",
            Country = "country",
            StateProvince = "State Province",
            HolidaySchedules = new List<HolidayScheduleDto>(),
            RegularSchedules = new List<RegularScheduleDto>(),

        };

        //Act
        var mappedLocationDto = _mapper.Map<LocationDto>(location);

        //Assert
        mappedLocationDto.Should().BeEquivalentTo(locationDto);
    }

    [Fact]
    public void ThenTaxonomyMapsToTaxonomyDto()
    {
        //Arrange
        Taxonomy taxonomy = new Taxonomy
        {
            id = "111",
            name = "name",
            vocabulary = "vocabulary"
        };

        TaxonomyDto taxonomyDto = new TaxonomyDto
        {
            Id = 111,
            Name = "name",
            TaxonomyType = TaxonomyType.ServiceCategory
        };

        //Act
        var mappedTaxonomyDto = _mapper.Map<TaxonomyDto>(taxonomy);

        //Assert
        mappedTaxonomyDto.Should().BeEquivalentTo(taxonomyDto);
    }
}
