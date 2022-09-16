const de = {
  // App
  languageDropDownText: 'Sprache',
  // No need to translate these languages
  de: 'Deutsch',
  en: 'English',
  appHome: 'cRPG',
  appCharacters: 'Charaktere',
  appShop: 'Shop',
  appClans: 'Clans',
  appAdministration: 'Administration',
  appSettings: 'Einstellungen',
  appSignOut: 'Abmelden',
  appDonate: 'Spende auf Patreon',
  appJoinDiscord: 'Tritt unserem Discord bei',
  appJoinForum: 'Tritt unserem Forum bei',
  appBuyGame: 'Kauf das Spiel',

  // Admin
  administrationRestrictions: 'Beschränkungen',
  administrationRestrictionColumnID: 'ID',
  administrationRestrictionColumnRestrictedUser: 'Benutzer',
  administrationRestrictionColumnCreatedAt: 'Erstellt am',
  administrationRestrictionColumnDuration: 'Dauer',
  administrationRestrictionColumnType: 'Art',
  administrationRestrictionColumnReason: 'Grund',
  administrationRestrictionColumnRestrictedByUser: 'von',

  // Character
  charactersCreateCharacter:
    'Um einen Charakter zu erstellen, tritt einfach einem cRPG Server bei.',
  charactersUsernameAndLevel: '{username} (lvl {userlevel})',
  characterComponentRepairDescription:
    'Einige deiner Gegenstände gehen möglicherweise am Ende einer Runde kaputt. Du kannst die automatische Reparatur aktivieren damit deine Gegenstände automatisch repariert werden.',
  characterComponentAutoRepairDescription:
    'Automatische Reparatur beschädigter Gegenstände (durchschnittliche Reparaturkosten {averageRepairCost} Gold)',
  characterComponentRespecialize: 'Zurücksetzen',
  characterComponentRespecializeDescription: 'Charakter zurücksetzen.',
  characterComponentRetire: 'Ruhestand',
  characterComponentRetireDescription:
    'Der Charakter wird auf Level 1 zurückgesetzt, bekommt einen Bonus-Multiplikator und einen Erbstückpunkt. Mit Erbstückpunkten lassen sich Gegenstände aufwerten. (lvl > 30)',
  characterComponentDelete: 'Löschen',
  characterComponentReplace: 'Ersetzen',
  characterComponentUnequip: 'Ablegen',
  characterComponentUpgrade: 'Aufwerten',
  characterComponentSell: 'Verkaufen',
  characterComponentNoItemOfType: 'Du besitzt keinen Gegenstand diesen Typs.',
  characterComponentReplaceWith: 'Ersetzen mit ',
  characterComponentHeirloomDisabled: 'Aufwerten ist aktuell deaktiviert',

  characterComponentRespecializeDialogTitle: 'Charakter zurücksetzen',
  characterComponentRespecializeDialogMessage:
    'Bist du sicher, dass du deinen Charakter {characterName} Level {characterLevel} zurücksetzen möchtest? Diese Aktion kann nicht rückgängig gemacht werden.',
  characterComponentRespecializeDialogConfirmText: 'Charakter zurücksetzen',
  characterComponentRespecializeDialogCancelText: 'Abbrechen',
  characterComponentRespecializeSuccess: 'Charakter zurückgesetzt',

  characterComponentRetireDialogTitle: 'Charakter in den Ruhestand entlassen',
  characterComponentRetireDialogMessage:
    'Bist du sicher, dass du deinen Charakter {characterName} Level {characterLevel} in den Ruhestand gehen lassen möchtest? Diese Aktion kann nicht rückgängig gemacht werden.',
  characterComponentRetireDialogConfirmText: 'Charakter in den Ruhestand entlassen',
  characterComponentRetireDialogCancelText: 'Abbrechen',
  characterComponentRetireSuccess: 'Ruhestand',

  characterComponentDeleteDialogTitle: 'Charakter löschen',
  characterComponentDeleteDialogMessage:
    'Bist du sicher, dass du deinen Charakter {characterName} Level {characterLevel} löschen möchtest? Diese Aktion kann nicht rückgängig gemacht werden.',
  characterComponentDeleteDialogConfirmText: 'Charakter löschen',
  characterComponentDeleteDialogCancelText: 'Abbrechen',
  characterComponentDeleteSuccess: 'Charakter gelöscht',

  characterStatsComponentAmountOfRetires:
    'Anzahl wie oft dieser Charakter in den Ruhestand gegangen ist.',
  characterStatsComponentGeneration: 'Generation',
  characterStatsComponentLevel: 'Level',
  characterStatsComponentExperience: 'Erfahrung',
  characterStatsComponentKDA: 'KDA',
  characterStatsComponentAttributes: 'Attribute ({attributePoints})',
  characterStatsComponentConvertAttributeToSkillPoints:
    'Konvertiere einen Attributspunkt in zwei Skillpunkte',
  characterStatsComponentStrength: 'Stärke',
  characterStatsComponentStrengthDescription1: 'Erhöht deine Lebenspunkte.',
  characterStatsComponentStrengthDescription2:
    'Erlaubt dir bessere Waffen und Rüstung zu verwenden.',
  characterStatsComponentAgility: 'Beweglichkeit',
  characterStatsComponentAgilityDescription:
    'Erhöht deine Waffenpunkte und lässt dich schneller bewegen.',
  characterStatsComponentSkills: 'Skills ({skillPoints})',
  characterStatsComponentConvertSkillToAttributePoints:
    'Konvertiere zwei Skillpunkte zu einem Attributspunkt',
  characterStatsComponentIronFlesh: 'Eisenhaut',
  characterStatsComponentIronFleshDescription1: 'Erhöht deine Lebenspunkte.',
  characterStatsComponentIronFleshDescription2:
    'und verringert die negativen Effekte, die Rüstungen auf Waffenpunkte haben',
  characterStatsComponentIronFleshDescription3: '. Benötigt drei Stärke pro Level.',
  characterStatsComponentPowerStrike: 'Schlagkraft',
  characterStatsComponentPowerStrikeDescription:
    'Erhöht deinen Nahkampfschaden. Benötigt drei Stärke pro Level.',
  characterStatsComponentPowerDraw: 'Kraftvoller Zug',
  characterStatsComponentPowerDrawDescription1:
    'Erhöht den Schaden, den du mit einem Bogen verursachst.',
  characterStatsComponentPowerDrawDescription2: 'Erlaubt dir die Benutzung besserer Bögen.',
  characterStatsComponentPowerDrawDescription3: 'Benötigt drei Stärke pro Level.',
  characterStatsComponentPowerThrow: 'Kraftvoller Wurf',
  characterStatsComponentPowerThrowDescription1:
    'Erhöht den Schaden, den du mit Wurfgegenständen verursachst.',
  characterStatsComponentPowerThrowDescription2:
    'Erlaubt dir die Benutzung besserer Wurfgegenstände.',
  characterStatsComponentPowerThrowDescription3: 'Benötigt drei Stärke pro Level.',
  characterStatsComponentAthletics: 'Athletik',
  characterStatsComponentAthleticsDescription:
    'Erhöht deine Laufgeschwindigkeit. Benötigt drei Beweglichkeit pro Level.',
  characterStatsComponentRiding: 'Reiten',
  characterStatsComponentRidingDescription1:
    'Erhöht deine Reitgeschwindigkeit, deine Manövrierfähigkeit zu Pferde und die Beschleunigung deines Pferdes.',
  characterStatsComponentRidingDescription2: 'Erlaubt dir das Reiten besserer Pferde.',
  characterStatsComponentRidingDescription3: 'Benötigt drei Beweglichkeit pro Level.',
  characterStatsComponentWeaponMaster: 'Waffenmeister',
  characterStatsComponentWeaponMasterDescription:
    'Gibt dir Waffenpunkte. Benötigt drei Beweglichkeit pro Level.',
  characterStatsComponentMountedArchery: 'Berittenes Bogenschießen',
  characterStatsComponentMountedArcheryDescription1:
    'Verringert den Malus für das Benutzen von Fernkampfwaffen auf einem Pferd um 10% pro Level.',
  characterStatsComponentMountedArcheryDescription2: 'Benötigt sechs Beweglichkeit pro Level.',
  characterStatsComponentShield: 'Schild',
  characterStatsComponentShieldDescription1:
    'Verbessert die Haltbarkeit von Schilden, die Geschwindigkeit und',
  characterStatsComponentShieldDescription2: 'verbessert die Deckung vor Fernkampfangriffen.',
  characterStatsComponentShieldDescription3: 'Erlaubt dir die Benutzung besserer Schilde.',
  characterStatsComponentShieldDescription4: 'Benötigt sechs Beweglichkeit pro Level.',
  characterStatsComponentWeaponProficiencies: 'Waffenfertigkeiten ({proficienciyPoints})',
  characterStatsComponentOneHanded: 'Einhänder',
  characterStatsComponentTwoHanded: 'Zweihänder',
  characterStatsComponentPolearm: 'Stangenwaffe',
  characterStatsComponentBow: 'Bogen',
  characterStatsComponentCrossbow: 'Armbrust',
  characterStatsComponentThrowing: 'Wurfgegenstand',
  characterStatsComponentReset: 'Zurücksetzen',
  characterStatsComponentCommit: 'Übernehmen',
  characterStatsComponentCharacterUpdateTitle: 'Charakter Einstellungen übernehmen',
  characterStatsComponentCharacterUpdateClose: 'Schließen',
  characterStatsComponentCharacterUpdateUpdate: 'Übernehmen',
  characterStatsComponentCharacterCharacteristicsUpdated: 'Charakter Einstellungen übernommen',
  characterStatsComponentCharacterUpdated: 'Charakter aktualisiert',

  // Clan
  clanApplications: 'Clan Anfragen',
  clanApplyToJoin: 'Beantrage Beitritt',
  clanName: 'Name',
  clanRole: 'Rolle',
  clanManageClanMember: 'Klicken um das Mitglied des Clans zu verwalten.',
  clanManageClanMemberWithMembername: 'Verwalte {username}',
  clanMember: 'Mitglied',
  clanOfficer: 'Offizier',
  clanLeader: 'Anführer',
  clanKickMember: 'Mitglied entlassen',
  clanApplicationSent: 'Anfrage gesendet',
  clanClanLeft: 'Clan verlassen',
  clanMemberKicked: 'Clan Mitglied entlassen',
  clanMemberUpdated: 'Mitglied aktualisiert',

  clanApplicationsApplicationsWithTagAndName: '[{clanTag}] {clanName} Anfragen',
  clanApplicationsName: 'Name',
  clanApplicationsNoApplications: 'Keine Anfragen',
  clanApplicationsApplicationAccepted: 'Anfrage angenommen',
  clanApplicationsApplicationDeclined: 'Anfrage abgelehnt',

  clanCreationCreateNewClan: 'Erstelle einen neuen Clan',
  clanCreationTag: 'Tag',
  clanCreationName: 'Name',
  clanCreationCreate: 'Anlegen',
  clanCreationCreated: 'Clan angelegt',
  clanCreationGenerateDescription1: 'Banner Schlüssel (generiere einen auf',
  clanCreationGenerateDescription2: 'bannerlord.party',
  clanCreationPrimaryColor: 'Primäre Farbe',
  clanCreationSecondaryColor: 'Sekundäre Farbe',

  clansTitle: 'Clans',
  clansSearch: 'Suche...',
  clansMyClan: 'Mein Clan',
  clansCreateNewClan: 'Erstelle einen neuen Clan',
  clansTag: 'Tag',
  clansName: 'Name',
  clansMembers: 'Mitglieder',
  clansNoClans: 'Keine Clans',

  // Home
  homeTitle: 'cRPG',
  homeDescriptionPart1: 'cRPG ist eine Mod für',
  homeDescriptionPartBannerlord: 'Mount & Blade II: Bannerlord',
  homeDescriptionPart2:
    '. Du startest als Bauer und entwickelst dich zu einem einzigartigen Charakter mit deinen bevorzugten Eigenschaften und den Waffen und Rüstungen deiner Wahl.',
  homeSignIn: 'Über Steam einloggen',
  homeFAQTitle: 'F.A.Q',
  homeQuestion1: 'Ist die Mod verfügbar?',
  homeAnswer1:
    'Die Mod befindet sich aktuell in der Beta. Sieh dir die Nachrichten in #bl-developement-updates auf unserem <a href="https://discord.gg/c-rpg" target="_blank">Discord Server</a> an um der Beta beizutreten.',
  homeQuestion2: 'Wie lege ich einen neuen Charakter an?',
  homeAnswer2:
    'Betritt einfach einen cRPG Server und ein Charakter mit Level 1 wird automatisch angelegt.',
  homeQuestion3: 'Wie bekomme ich Erfahrungspunkte und Gold?',
  homeAnswer3:
    'Erfahrungspunkte und Gold bekommst du jede Minute während du auf einem cRPG Server spielst.',
  homeQuestion4:
    'Behalte ich meinen Fortschritt (Erfahrungspunkte, Gold) bis zum offiziellen Release von cRPG?',
  homeAnswer4: 'Sobald cRPG offiziell released ist, wird die Datenbank zurückgesetzt.',
  homeQuestion5:
    'Ich habe meine Steam Account umbenannt und es wurde ein neuer Charakter mit Level 1. Wie kann ich meinen originalen Charakter wieder spielen?',
  homeAnswer5:
    'Wenn du dich auf einen cRPG server verbindest, wird nach einem Charakter gesucht der mit deinem Steamname übereinstimmt. Wird kein Charaketer gefunden' +
    ', wird ein neuer angelegt. Um deinen originalen Charakter wieder zu spielen, musst du auf der Webseite deinen Charakter umbennen zu deinem Steamnamen.',
  homeQuestion6: 'Was ist der Ruhestand?',
  homeAnswer6:
    'Wenn ein cRPG Charakter Level 31 erreicht, kann er in den Ruhestand gehen. Ein zur Ruhe gesetzter Character wird auf Level 1 zurückgesetzt aber erhält dafür' +
    ' einen Multiplikator auf Erfahrungspunkte und einen Erbstückpunkt, der verwendet werden kann um einen Gegenstand aufzuwerten.',
  homeQuestion7: 'Was bedeutet es meinen Charakter zurückzusetzen?',
  homeAnswer7:
    'Wenn dir deine Attributs-, Skill- und Waffenpunkte nicht mehr zusagen, kannst du deinen Charakter zurücksetzen. Dein Charakter verliert dadurch die Hälfte seiner aktuellen Erfahrungspunkte.',
  homeQuestion8: 'Kann ich meinen eigenen cRPG Server hosten?',
  homeAnswer8:
    'Aus Durchführbarkeits- und Sicherheitsgründen wird es Spieler nicht erlaubt sein selbst cRPG Server zu hosten.',
  homeQuestion9: 'Kann ich cRPG spielen wenn ich das Spiel im Epic oder GOG Store gekauft habe?',
  homeAnswer9: 'Nein, aktuell wird nur Steam unterstützt aber das könnte sich in Zukunft ändern.',
  homeQuestion10: 'Ist das Project open-source?',
  homeAnswer10: 'Die Entscheidung darüber steht noch aus.',
  homeQuestion11: 'Wie kann ich helfen?',
  homeAnswer11:
    'Wir suchen noch nach:<ul style="margin-top: 0">' +
    '<li>Game developer (.NET)</li>' +
    '<li>UX Designer für diese Webseite</li>' +
    '<li>Frontend developer (Vue.js) für diese Webseite</li>' +
    '</ul>Wenn du anders helfen willst, hast du die Möglichkeit über Patreon mit einer Spende zu helfen.',
  homeQuestion12: 'Wie kann ich spenden?',
  homeAnswer12:
    'Du kannst über <a href="https://patreon.com/crpg" target="_blank">Patreon</a> spenden. Beachte, dass alle Spenden nur verwendet werden um die Serverkosten abzudecken.',

  // Settings
  settingsRestrictions: 'Beschränkungen',
  settingsDeleteAccount: 'Account löschen',
  settingsDeleteAccountDescription:
    'Löscht deinen Account, deinen Charakter, deine Gegenstände, dein Gold und deinen gesamten Fortschritt.',
  settingsDeleteAccountButton: 'Account löschen',
  settingsRestrictionColumnID: 'ID',
  settingsRestrictionColumnRestrictedUser: 'Benutzer',
  settingsRestrictionColumnDuration: 'Dauer',
  settingsRestrictionColumnType: 'Art',
  settingsRestrictionColumnReason: 'Grund',
  settingsRestrictionColumnRestrictedByUser: 'Von',
  settingsDeleteDialogTitle: 'Account löschen',
  settingsDeleteDialogMessage:
    'Bist du sicher, dass du deinen Account löschen willst? Diese Aktion kann nicht rückgängig gemacht werden.',
  settingsDeleteDialogConfirmText: 'Account löschen',
  settingsDeleteDialogCancelText: 'Abbrechen',

  // Shop
  shopBoughtItem: '{itemName} gekauft für {itemPrice} Gold',
  shopAlreadyOwn: 'Du besitzt diesen Gegenstand bereits',
  shopNotEnoughMoney: 'Nicht genug Gold',

  shopFiltersFormCulture: 'Kultur',
  shopFiltersFormAny: 'Alle',
  shopFiltersFormItemType: 'Art',
  shopFiltersFormItemTypeUndefined: 'Undefined',
  shopFiltersFormItemTypeHeadArmor: 'Kopf Rüstungen',
  shopFiltersFormItemTypeShoulderArmor: 'Schulter Rüstungen',
  shopFiltersFormItemTypeBodyArmor: 'Körper Rüstungen',
  shopFiltersFormItemTypeHandArmor: 'Hand Rüstungen',
  shopFiltersFormItemTypeLegArmor: 'Bein Rüstungen',
  shopFiltersFormItemTypeMountHarness: 'Pferdegeschirre',
  shopFiltersFormItemTypeMount: 'Pferde',
  shopFiltersFormItemTypeShield: 'Schilde',
  shopFiltersFormItemTypeBow: 'Bögen',
  shopFiltersFormItemTypeCrossbow: 'Armbrüste',
  shopFiltersFormItemTypeOneHandedWeapon: 'Einhänder',
  shopFiltersFormItemTypeTwoHandedWeapon: 'Zweihänder',
  shopFiltersFormItemTypePolearm: 'Stangenwaffen',
  shopFiltersFormItemTypeThrown: 'Wurfgegenstände',
  shopFiltersFormItemTypeArrows: 'Pfeile',
  shopFiltersFormItemTypeBolts: 'Bolzen',
  shopFiltersFormItemTypePistol: 'Pistolen',
  shopFiltersFormItemTypeMusket: 'Musketen',
  shopFiltersFormItemTypeBullets: 'Kugeln',
  shopFiltersFormItemTypeBanner: 'Banner',
  shopFiltersShowOwnedItems: 'Zeige Gegenstände im Besitz',
  shopFiltersShowAffordableItems: 'Zeite nur bezahlbare Gegenstände',

  // Strategus
  strategusMoveDialogMove: 'Bewegen',
  strategusMoveDialogFollow: 'Folgen',
  strategusMoveDialogAttack: 'Angreifen',

  strategusRegistrationDialogWelcome: 'Willkommen zu Strategus',
  strategusRegistrationDialogDescription:
    'Strategus ist eine Multiplayer Kampagne für cRPG bei der Spieler Land und Lehen erwerben und ' +
    'Armeen auf einer Echtzeit Karte sammeln können. Strategus erweitert die Welt von cRPG um eine browserbasierende Karte ' +
    'von Calradia, bei der Spieler ihre Armeen gegen andere Spieler führen oder sich als Söldner in Schlachten auf den cRPG Servern ausleben können.',
  strategusRegistrationDialogSelectRegion: 'Um zu starten, musst du zuerst eine Region auswählen.',
  strategusRegistrationDialogSelectRegionPlaceholder: 'Wähle eine Region',

  Europe: 'Europa',
  'North America': 'Nord Amerika',
  Asia: 'Asien',

  strategusSettlementDialogSettlementCulture: 'Kultur: {settlementCulture}',
  strategusSettlementDialogGarrison: 'Garnison: 2515',
  strategusSettlementDialogStartRecruitingTroops: 'Starte Rekrutierung',
  strategusSettlementDialogStopRecruitingTroops: 'Stoppe Rekrutierung',

  // Other
  notFoundTitle: '404 Not Found',
  notFoundDescription: 'Die angeforderte Seite konnte nicht gefunden werden',
  sessionExpired: 'Session abgelaufen',
};

export default de;
