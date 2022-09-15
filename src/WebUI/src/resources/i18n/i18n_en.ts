const en = {
  // App
  languageDropDownText: 'Language',
  de: 'Deutsch',
  en: 'English',
  appHome: 'cRPG',
  appCharacters: 'Characters',
  appShop: 'Shop',
  appClans: 'Clans',
  appAdministration: 'Administration',
  appSettings: 'Settings',
  appSignOut: 'Sign out',
  appDonate: 'Donate on Patreon',
  appJoinDiscord: 'Join our Discord',
  appJoinForum: 'Join our Forum',
  appBuyGame: 'Buy the game',

  // Admin
  administrationRestrictions: 'Restrictions',
  administrationRestrictionColumnID: 'ID',
  administrationRestrictionColumnRestrictedUser: 'User',
  administrationRestrictionColumnCreatedAt: 'Created At',
  administrationRestrictionColumnDuration: 'Duration',
  administrationRestrictionColumnType: 'Type',
  administrationRestrictionColumnReason: 'Reason',
  administrationRestrictionColumnRestrictedByUser: 'By',

  // Character
  charactersCreateCharacter: 'To create a character, simply join a cRPG server.',
  charactersUsernameAndLevel: '{username} (lvl {userlevel})',
  characterComponentRepairDescription:
    "Some of your items might break at the end of a round. Switch automatic repair on so you don't have to repair manually.",
  characterComponentAutoRepairDescription:
    'Automatically repair damaged items (average repair cost {averageRepairCost} gold)',
  characterComponentRespecialize: 'Respecialize',
  characterComponentRespecializeDescription: 'Respecialize character.',
  characterComponentRetire: 'Retire',
  characterComponentRetireDescription:
    'Reset character to level 1 to grant a bonus multiplier and an heirloom point. (lvl > 30)',
  characterComponentDelete: 'Delete',
  characterComponentReplace: 'Replace',
  characterComponentUnequip: 'Unequip',
  characterComponentUpgrade: 'Upgrade',
  characterComponentSell: 'Sell',
  characterComponentNoItemOfType: "You don't own any item for this type.",
  characterComponentReplaceWith: 'Replace with ',
  characterComponentHeirloomDisabled: 'Heirloom are disabled for now',

  characterComponentRespecializeDialogTitle: 'Respecialize character',
  characterComponentRespecializeDialogMessage:
    'Are you sure you want to respecialize your character {characterName} lvl. {characterLevel}? This action cannot be undone.',
  characterComponentRespecializeDialogConfirmText: 'Respecialize Character',
  characterComponentRespecializeDialogCancelText: 'Cancel',
  characterComponentRespecializeSuccess: 'Character respecialized',

  characterComponentRetireDialogTitle: 'Retiring character',
  characterComponentRetireDialogMessage:
    'Are you sure you want to retire your character {characterName} lvl. {characterLevel}? This action cannot be undone.',
  characterComponentRetireDialogConfirmText: 'Retire',
  characterComponentRetireDialogCancelText: 'Cancel',
  characterComponentRetireSuccess: 'Character retired',

  characterComponentDeleteDialogTitle: 'Deleting character',
  characterComponentDeleteDialogMessage:
    'Are you sure you want to delete your character {characterName} lvl. {characterLevel}? This action cannot be undone.',
  characterComponentDeleteDialogConfirmText: 'Delete Character',
  characterComponentDeleteDialogCancelText: 'Cancel',
  characterComponentDeleteSuccess: 'Character deleted',

  characterStatsComponentAmountOfRetires: 'Number of times you retired this character.',
  characterStatsComponentGeneration: 'Generation',
  characterStatsComponentLevel: 'Level',
  characterStatsComponentExperience: 'Experience',
  characterStatsComponentKDA: 'KDA',
  characterStatsComponentAttributes: 'Attributes ({attributePoints})',
  characterStatsComponentConvertAttributeToSkillPoints:
    'Convert 1 attribute point to 2 skill points',
  characterStatsComponentStrength: 'Strength',
  characterStatsComponentStrengthDescription1: 'Increases your health points.',
  characterStatsComponentStrengthDescription2: 'Allows you to use higher tier weapons and armor.',
  characterStatsComponentAgility: 'Agility',
  characterStatsComponentAgilityDescription:
    'Increases your weapon points and makes you move a bit faster.',
  characterStatsComponentSkills: 'Skills ({skillPoints})',
  characterStatsComponentConvertSkillToAttributePoints:
    'Convert 2 skill points to 1 attribute point',
  characterStatsComponentIronFlesh: 'Iron Flesh',
  characterStatsComponentIronFleshDescription1: 'Increases your health',
  characterStatsComponentIronFleshDescription2:
    'and reduces the negative impact armor has on weapon points',
  characterStatsComponentIronFleshDescription3: '. Requires 3 strength per level.',
  characterStatsComponentPowerStrike: 'Power Strike',
  characterStatsComponentPowerStrikeDescription:
    'Increases melee damage. Requires 3 strength per level.',
  characterStatsComponentPowerDraw: 'Power Draw',
  characterStatsComponentPowerDrawDescription1: 'Increases bow damage.',
  characterStatsComponentPowerDrawDescription2: 'Allows you to use higher tiers bow.',
  characterStatsComponentPowerDrawDescription3: 'Requires 3 strength per level.',
  characterStatsComponentPowerThrow: 'Power Throw',
  characterStatsComponentPowerThrowDescription1: 'Increases throw damage.',
  characterStatsComponentPowerThrowDescription2: 'Allows you to use higher tier weapons.',
  characterStatsComponentPowerThrowDescription3: 'Requires 3 strength per level.',
  characterStatsComponentAthletics: 'Athletics',
  characterStatsComponentAthleticsDescription:
    'Increases running speed. Requires 3 agility per level.',
  characterStatsComponentRiding: 'Riding',
  characterStatsComponentRidingDescription1: 'Increases riding speed, acceleration and maneuver.',
  characterStatsComponentRidingDescription2: 'Allows you to ride higher tier mounts.',
  characterStatsComponentRidingDescription3: 'Requires 3 agility per level.',
  characterStatsComponentWeaponMaster: 'Weapon Master',
  characterStatsComponentWeaponMasterDescription:
    'Gives weapon points. Requires 3 agility per level.',
  characterStatsComponentMountedArchery: 'Mounted Archery',
  characterStatsComponentMountedArcheryDescription1:
    'Reduces penalty for using ranged weapons on a moving mount by 10% per level.',
  characterStatsComponentMountedArcheryDescription2: 'Requires 6 agility per level.',
  characterStatsComponentShield: 'Shield',
  characterStatsComponentShieldDescription1: 'Improves shield durability, shield speed and',
  characterStatsComponentShieldDescription2: 'increases coverage from ranged attacks.',
  characterStatsComponentShieldDescription3: 'Allows you to use higher tier shields.',
  characterStatsComponentShieldDescription4: 'Requires 6 agility per level.',

  // Clan
  clanApplications: 'Clan Applications',
  clanApplyToJoin: 'Apply to join',
  clanName: 'Name',
  clanRole: 'Role',
  clanManageClanMember: 'Click to manage this member of the clan.',
  clanManageClanMemberWithMembername: 'Managing {username}',
  clanMember: 'Member',
  clanOfficer: 'Officer',
  clanLeader: 'Leader',
  clanKickMember: 'Kick Member',
  clanApplicationSent: 'Application sent!',
  clanClanLeft: 'Clan left',
  clanMemberKicked: 'Clan member kicked',
  clanMemberUpdated: 'Member updated',

  clanApplicationsApplicationsWithTagAndName: '[{clanTag}] {clanName} Applications',
  clanApplicationsName: 'Name',
  clanApplicationsNoApplications: 'No applications',
  clanApplicationsApplicationAccepted: 'Application accepted',
  clanApplicationsApplicationDeclined: 'Application declined',

  clanCreationCreateNewClan: 'Create a new clan',
  clanCreationTag: 'Tag',
  clanCreationColor: 'Color',
  clanCreationName: 'Name',
  clanCreationCreate: 'Create',
  clanCreationCreated: 'Clan created',

  clansTitle: 'Clans',
  clansSearch: 'Search...',
  clansMyClan: 'My clan',
  clansCreateNewClan: 'Create new clan',
  clansTag: 'Tag',
  clansName: 'Name',
  clansMembers: 'Members',
  clansNoClans: 'No clans',

  // Home
  homeTitle: 'cRPG',
  homeDescriptionPart1: 'cRPG is a mod for',
  homeDescriptionPartBannerlord: 'Mount & Blade II: Bannerlord',
  homeDescriptionPart2:
    ". It adds persistence to the multiplayer. You start as a peasant and you'll develop your unique character with different stats and items.",
  homeSignIn: 'Sign in through Steam',
  homeFAQTitle: 'F.A.Q',
  homeQuestion1: 'Is the mod available?',
  homeAnswer1:
    'The mod is currently in beta. See the message in #bl-developement-updates on the <a href="https://discord.gg/c-rpg" target="_blank">Discord</a> to join it.',
  homeQuestion2: 'How to create a new character?',
  homeAnswer2: 'Simply join a cRPG server and a level 1 character will be created.',
  homeQuestion3: 'How to gain experience and gold?',
  homeAnswer3: 'Experience and gold are gained every minute by playing on a cRPG server.',
  homeQuestion4: 'Will my progress (experience, gold) be saved until the official release of cRPG?',
  homeAnswer4: 'Once cRPG is officialy released, the database will be wiped.',
  homeQuestion5:
    "I've renamed my steam account and a new character level 1 was created, how can I play with my original character?",
  homeAnswer5:
    "When you connect to a cRPG server, it searches for a character with the same name as your steam name. If it doesn't find" +
    ' one, a new character is created. To use your original character, rename it to your new steam name it in the Web UI.',
  homeQuestion6: 'What is retiring?',
  homeAnswer6:
    'When a cRPG character reaches level 31, it can be retired. Retiring a character resets it to level 1 but grants an' +
    ' experience bonus multiplier and an heirloom point which can used on an item to increase its characteristics.',
  homeQuestion7: 'What is respecialization?',
  homeAnswer7:
    'If you want to play a different class of soldier, you can respecialize (or respec) to reset your character stats for the' +
    ' cost of half of your experience.',
  homeQuestion8: 'Can I host my own cRPG game server?',
  homeAnswer8:
    "For security and operability reasons, players won't be able to host their own server.",
  homeQuestion9: 'Can I play if I bought the game in the Epic or GOG Store?',
  homeAnswer9:
    'No, only Steam will be supported for now but having other platforms was thought in the early design of cRPG.',
  homeQuestion10: 'Is the project open-source?',
  homeAnswer10: 'Not decided yet.',
  homeQuestion11: 'How can I help?',
  homeAnswer11:
    'We\'re looking for:<ul style="margin-top: 0">' +
    '<li>Game developer (.NET)</li>' +
    '<li>UX designer for this website</li>' +
    '<li>Front-end developer (Vue.js) for this website</li>' +
    "</ul>If you don't have any of the above skills you can also donate on the Patreon.",
  homeQuestion12: 'How to donate?',
  homeAnswer12:
    'You can donate on the <a href="https://patreon.com/crpg" target="_blank">Patreon</a>. Note that donations will only be used' +
    ' to cover server costs.',

  // Settings
  settingsRestrictions: 'Restrictions',
  settingsDeleteAccount: 'Delete account',
  settingsDeleteAccountDescription:
    'Make your character, items, gold and all your progression disappear.',
  settingsDeleteAccountButton: 'Delete your account',
  settingsRestrictionColumnID: 'ID',
  settingsRestrictionColumnRestrictedUser: 'User',
  settingsRestrictionColumnDuration: 'Duration',
  settingsRestrictionColumnType: 'Type',
  settingsRestrictionColumnReason: 'Reason',
  settingsRestrictionColumnRestrictedByUser: 'By',
  settingsDeleteDialogTitle: 'Deleting account',
  settingsDeleteDialogMessage:
    'Are you sure you want to delete your account? This action cannot be undone.',
  settingsDeleteDialogConfirmText: 'Delete account',
  settingsDeleteDialogCancelText: 'Cancel',

  // Shop
  shopBoughtItem: 'Bought {itemName} for {itemPrice} gold',
  shopAlreadyOwn: 'You already own this item',
  shopNotEnoughMoney: 'Not enough gold',

  // Strategus
  strategusMoveDialogMove: 'Move',
  strategusMoveDialogFollow: 'Follow',
  strategusMoveDialogAttack: 'Attack',

  strategusRegistrationDialogWelcome: 'Welcome to Strategus',
  strategusRegistrationDialogDescription:
    'Strategus is a multiplayer campaign for cRPG where players can aquire fiefs and land and ' +
    'gather armies on a real-time map. Strategus expands the world of cRPG to a browser-based map ' +
    'of Calradia, where players take their armies into battle against other players or serve as a ' +
    'mercenary in scheduled battles on the cRPG servers.',
  strategusRegistrationDialogSelectRegion: 'To start playing, you must first select your region.',
  strategusRegistrationDialogSelectRegionPlaceholder: 'Select a region',

  Europe: 'Europe',
  'North America': 'North America',
  Asia: 'Asia',

  strategusSettlementDialogSettlementCulture: 'Culture: {settlementCulture}',
  strategusSettlementDialogGarrison: 'Garrison: 2515',
  strategusSettlementDialogStartRecruitingTroops: 'Start recruiting troops',
  strategusSettlementDialogStopRecruitingTroops: 'Stop recruiting troops',

  // Other
  notFoundTitle: '404 Not Found',
  notFoundDescription: 'The page you requested could not be found',
  sessionExpired: 'Session expired',
};

export default en;
