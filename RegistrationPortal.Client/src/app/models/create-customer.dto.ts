export interface CreateCustomerDto {
    // Customer Information
    branchCCode: string;
    custCName: string;
    custCFname?: string;
    custCSname?: string;
    custCTname?: string;
    custCFoname?: string;
    custCMname?: string;
    custCSex?: string;
    custCReligion?: string;
    custCCaste?: string;
    custCMaritalsts?: string;
    custCPadd1?: string;
    custCPadd2?: string;
    custCPadd3?: string;
    custCPCity?: string;
    custCPstate?: string;
    mobileCNo?: string;
    emailCAdd?: string;
    idCType?: string;
    idCNo?: string;
    idCIssplace?: string;
    idDIssdate?: Date;
    idDExpdate?: Date;
    custCAuthority?: string;
    husbCName?: string;
    countryCCode?: string;
    placeCBirth?: string;
    custCNationality?: string;
    custCWife1?: string;
    idCType2?: string;
    idCNo2?: string;
    idCIssplace2?: string;
    idCIssueDate2?: string;
    idCExpiryDate2?: string;
    custDBdate?: Date;
    idCAuthority2?: string;
    custCOccupation?: string;
    homeINumber?: number;
    custIIdentify?: string;
    custCCountrybrith?: string;
    custCStatebrith?: string;
    custCCitizenship?: string;
    custCEmployer?: string;
    custFIncome?: number;
    custCHigheducation?: string;
    custFAvgmonth?: number;
    tradeCNameenglish?: string;
    custCEngfname?: string;
    custCEngsname?: string;
    custCEngtname?: string;
    custCEngfoname?: string;
    custDEntrydt?: Date;
    accountMasts?: CreateAccountDto[];
    
    // FATCA Information
    isUsPerson?: string;
    citizenshipCountries?: string;
    hasImmigrantVisa?: string;
    permanentResidencyStates?: string;
    hasOtherCountriesResidency?: string;
    soleSudanResidencyConfirmed?: string;
    ssn?: string;
    itin?: string;
    atin?: string;
    country1TaxJurisdiction?: string;
    country1TIN?: string;
    country1NoTinReason?: string;
    country1NoTinExplanation?: string;
    country2TaxJurisdiction?: string;
    country2TIN?: string;
    country2NoTinReason?: string;
    country2NoTinExplanation?: string;
    country3TaxJurisdiction?: string;
    country3TIN?: string;
    country3NoTinReason?: string;
    country3NoTinExplanation?: string;
    
    // Document Attachments
    identification?: File;
    nationalId?: File;
    personalImage?: File;
    imageFortheRequesterHoldingTheID?: File;
    signitureTemplate?: File;
    handwrittenRequest?: File;
    employmentCertificate?: File;
}

export interface CreateAccountDto {
    branchCCode: string;
    actCType: string;
    currencyCCode: string;
    actCIntrotype?: string;
    introCRem?: string;
    actIIntroid?: number;
    actITrbrcode?: number;
    custINo?: number;
}
