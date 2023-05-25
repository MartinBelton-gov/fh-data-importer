namespace SportEngland.Services
{
    public class SportEnglandModel
    {
        public Item[] items { get; set; } = default!;
        public string next { get; set; } = default!;
        public string license { get; set; } = default!;
    }

    public class Item
    {
        public string id { get; set; } = default!;
        public string state { get; set; } = default!;
        public string kind { get; set; } = default!;
        public int modified { get; set; }
        public Data data { get; set; } = default!;
    }

    public class Data
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public string outputAreaCode { get; set; } = default!;
        public string lowerSuperOutputArea { get; set; } = default!;
        public string middleSuperOutputArea { get; set; } = default!;
        public string parliamentaryConstituencyCode { get; set; } = default!;
        public string parliamentaryConstituencyName { get; set; } = default!;
        public string wardCode { get; set; } = default!;
        public string wardName { get; set; } = default!;
        public string localAuthorityCode { get; set; } = default!;
        public string localAuthorityName { get; set; } = default!;
        public string coreCityName { get; set; } = default!;
        public string metroName { get; set; } = default!;
        public string countyCode { get; set; } = default!;
        public string countyName { get; set; } = default!;
        public string ldpCode { get; set; } = default!;
        public string ldpName { get; set; } = default!;
        public string activePartnershipCode { get; set; } = default!;
        public string activePartnershipName { get; set; } = default!;
        public string regionCode { get; set; } = default!;
        public string regionName { get; set; } = default!;
        public float easting { get; set; }
        public float northing { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string subBuildingName { get; set; } = default!;
        public string buildingName { get; set; } = default!;
        public int buildingNumber { get; set; }
        public string dependentThoroughfare { get; set; } = default!;
        public string thoroughfareName { get; set; } = default!;
        public string doubleDependentLocality { get; set; } = default!;
        public string dependentLocality { get; set; } = default!;
        public string postTown { get; set; } = default!;
        public string postcode { get; set; } = default!;
        public long uprn { get; set; }
        public string toid { get; set; } = default!;
        public bool? hasCarPark { get; set; }
        public int carParkCapacity { get; set; }
        public bool? cyclePark { get; set; }
        public bool? cycleHire { get; set; }
        public bool? cycleRepairWorkshop { get; set; }
        public bool? nursery { get; set; }
        public bool? firstAidRoom { get; set; }
        public Managementtype managementType { get; set; } = default!;
        public Managementgroup managementGroup { get; set; } = default!;
        public string operatorName { get; set; } = default!;
        public Ownertype ownerType { get; set; } = default!;
        public Ownergroup ownerGroup { get; set; } = default!;
        public Educationphase educationPhase { get; set; } = default!;
        public DateTime createdOn { get; set; }
        public DateTime auditedOn { get; set; }
        public DateTime checkedOn { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? closureDate { get; set; }
        public Closurereason closureReason { get; set; } = default!;
        public object closureNotes { get; set; } = default!;
        public object[] comments { get; set; } = default!;
        public Alias[] aliases { get; set; } = default!;
        public Equipment equipment { get; set; } = default!;
        public Activity[] activities { get; set; } = default!;
        public Contact[] contacts { get; set; } = default!;
        public Facility[] facilities { get; set; } = default!;
        public Disability disability { get; set; } = default!;
        public string publicNotes { get; set; } = default!;
    }

    public class Managementtype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Managementgroup
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Ownertype
    {
        public int? id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Ownergroup
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Educationphase
    {
        public int? id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Closurereason
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Equipment
    {
        public HastabletennistableS hasTableTennisTableS { get; set; } = default!;
        public HascricketbowlingmachineS hasCricketBowlingMachineS { get; set; } = default!;
    }   

    public class HastabletennistableS
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class HascricketbowlingmachineS
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Disability
    {
        public bool? access { get; set; }
        public string notes { get; set; } = default!;
        public bool? changingPlacesToiletsExist { get; set; }
        public Equipped equipped { get; set; } = default!;
    }

    public class Equipped
    {
        public bool parking { get; set; }
        public bool findingReachingEntrance { get; set; }
        public bool receptionArea { get; set; }
        public bool doorways { get; set; }
        public bool changingFacilities { get; set; }
        public bool activityAreas { get; set; }
        public bool toilets { get; set; }
        public bool socialAreas { get; set; }
        public bool spectatorAreas { get; set; }
        public bool emergencyExits { get; set; }
    }

    public class Alias
    {
        public string name { get; set; } = default!;
    }

    public class Activity
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
    }

    public class Contact
    {
        public int id { get; set; }
        public Contacttype contactType { get; set; } = default!;
        public string title { get; set; } = default!;
        public string forename { get; set; } = default!;
        public string surname { get; set; } = default!;
        public string designation { get; set; } = default!;
        public string email { get; set; } = default!;
        public string telephone { get; set; } = default!;
        public string website { get; set; } = default!;
    }

    public class Contacttype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Facility
    {
        public int id { get; set; }
        public Facilitytype facilityType { get; set; } = default!;
        public Facilitysubtype facilitySubType { get; set; } = default!;
        public Accessibility accessibility { get; set; } = default!;
        public Status status { get; set; } = default!;
        public string statusNotes { get; set; } = default!;
        public object expectedOpening { get; set; } = default!;
        public Managementtype1 managementType { get; set; } = default!;
        public Managementgroup1 managementGroup { get; set; } = default!;
        public object operatorName { get; set; } = default!;
        public Accessibilitygroup accessibilityGroup { get; set; } = default!;
        public int yearBuilt { get; set; }
        public bool yearBuiltEstimated { get; set; }
        public bool? isRefurbished { get; set; }
        public int yearRefurbished { get; set; }
        public bool? hasChangingRooms { get; set; }
        public bool? areChangingRoomsRefurbished { get; set; }
        public int yearChangingRoomsRefurbished { get; set; }
        public Timingstype timingsType { get; set; } = default!;
        public DateTime? createdOn { get; set; }
        public DateTime checkedOn { get; set; }
        public DateTime? startDate { get; set; } = default!; //was object
        public DateTime? openingDate { get; set; }
        public DateTime? closureDate { get; set; }
        public Closurereason1 closureReason { get; set; } = default!;
        public object closureNotes { get; set; } = default!;
        public object[] comments { get; set; } = default!;
        public Openingtime[] openingTimes { get; set; } = default!;
        public Facilityspecifics facilitySpecifics { get; set; } = default!;
        public bool meetsActivePlacesCriteria { get; set; }
        public object[] facilityCriteriaExceptions { get; set; } = default!;
        public Disability1 disability { get; set; } = default!;
        public Seasonalitytype seasonalityType { get; set; } = default!;
        public string seasonalityStart { get; set; } = default!;
        public string seasonalityEnd { get; set; } = default!;
    }

    public class Facilitytype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Facilitysubtype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Accessibility
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Status
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Managementtype1
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Managementgroup1
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Accessibilitygroup
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Timingstype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Closurereason1
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Facilityspecifics
    {
        public bool floodlit { get; set; }
        public bool hybrid { get; set; }
        public bool overmarked { get; set; }
        public int pitches { get; set; }
        public int holes { get; set; }
        public float length { get; set; }
        public int bays { get; set; }
        public int stations { get; set; }
        public int courts { get; set; }
        public bool doubles { get; set; }
        public bool movableWall { get; set; }
    }

    public class Disability1
    {
        public bool? access { get; set; }
        public string notes { get; set; } = default!;
        public bool? changingPlacesToiletsExist { get; set; }
        public Equipped1 equipped { get; set; } = default!;
    }

    public class Equipped1
    {
        public bool parking { get; set; }
        public bool findingReachingEntrance { get; set; }
        public bool receptionArea { get; set; }
        public bool doorways { get; set; }
        public bool changingFacilities { get; set; }
        public bool activityAreas { get; set; }
        public bool toilets { get; set; }
        public bool socialAreas { get; set; }
        public bool spectatorAreas { get; set; }
        public bool emergencyExits { get; set; }
    }

    public class Seasonalitytype
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Openingtime
    {
        public Accessdescription accessDescription { get; set; } = default!;
        public string openingTime { get; set; } = default!;
        public string closingTime { get; set; } = default!;
        public Periodopenfor periodOpenFor { get; set; } = default!;
    }

    public class Accessdescription
    {
        public int? id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }

    public class Periodopenfor
    {
        public int id { get; set; }
        public string name { get; set; } = default!;
        public int lookupId { get; set; }
    }


}
