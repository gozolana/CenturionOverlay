import type { Zone } from 'ffxiv-lib'

interface ICenturionEvent {
  type: string
  timestamp: string
}

interface IPlayerLoggedInEvent extends ICenturionEvent {
  playerId: number
  playerName: string
  playerWorldId: number
  worldId: number
  zoneId: number
  zoneName: string
}

interface IPlayerLoggedOutEvent extends ICenturionEvent {}

interface IZoneChangedEvent extends ICenturionEvent {
  worldId: number
  zoneId: number
  zoneName: string
}

interface IZoneInstanceChangedEvent extends ICenturionEvent {
  zoneName: string
  instance: number
}

interface ILocationNotifiedEvent extends ICenturionEvent {
  logType: string
  message: string
  pc: string
  zoneName: string
  instance: number
  x: number
  y: number
}

interface IMobFAEvent extends ICenturionEvent {
  message: string
  id: number
  mobId: number
  mobName: string
  action: string
  attacker: string
}

interface IMobTriggerEvent extends ICenturionEvent {
  triggerType: string
  worldId: number
  zoneId: number
  message: string
}

interface IMobLocationEvent extends ICenturionEvent {
  worldId: number
  zoneId: number
  id: number
  mobId: number
  x: number
  y: number
  z: number
}

interface IMobStateChangedEvent extends ICenturionEvent {
  state: string
  worldId: number
  zoneId: number
  id: number
  mobId: number
  x: number
  y: number
  z: number
  distance: number
  hpp: number
}

interface BaseCombatant {
  ID: number
  Name: string
  OwnerID: number
  TargetID: number
  BNpcID: number
  BNpcNameID: number
  type: number
}

interface PosCombatant extends BaseCombatant {
  PosX: number
  PosY: number
  PosZ: number
  Heading: number
  Distance: number
}

interface CharCombatant extends PosCombatant {
  Job: number
  Level: number
  WorldID: number
  WorldName: string
  CurrentHP: number
  MaxHP: number
  HPP: number
  PartyType: number
  Order: number
}

interface IAetheryte {
  posX: number
  posY: number
  posZ: number
  x: number
  y: number
  z: number
  heading: number
  distance: number
  id: number
  name: string
  ownerId: number
  targetId: number
  bNpcId: number
  bNpcNameId: number
  type: number
}

interface ICharacter extends IAetheryte {
  jobId: number
  level: number
  worldID: number
  worldName: string
  currentHp: number
  maxHp: number
  hpp: number
  partyType: number
  order: number
}

interface IAetheryte {
  posX: number
  posY: number
  posZ: number
  x: number
  y: number
  z: number
  heading: number
  distance: number
  id: number
  name: string
  ownerId: number
  targetId: number
  bNpcId: number
  bNpcNameId: number
  type: number
}

const toCharacter = (
  combatant: CharCombatant,
  zone: Zone | undefined
): ICharacter => {
  const [x, y, z] = zone
    ? zone.toLocalPosXYZ({
        x: combatant.PosX,
        y: combatant.PosY,
        z: combatant.PosZ
      })
    : [combatant.PosX, combatant.PosY, combatant.PosZ]
  return {
    posX: combatant.PosX,
    posY: combatant.PosY,
    posZ: combatant.PosZ,
    x,
    y,
    z,
    heading: combatant.Heading,
    distance: combatant.Distance,
    id: combatant.ID,
    name: combatant.Name,
    ownerId: combatant.OwnerID,
    targetId: combatant.TargetID,
    bNpcId: combatant.BNpcNameID,
    bNpcNameId: combatant.BNpcNameID,
    type: combatant.type,
    jobId: combatant.Job,
    level: combatant.Level,
    worldID: combatant.WorldID,
    worldName: combatant.WorldName,
    currentHp: combatant.CurrentHP,
    maxHp: combatant.MaxHP,
    hpp: combatant.HPP,
    partyType: combatant.PartyType,
    order: combatant.Order
  }
}

interface ICombatDataEvent extends ICenturionEvent {
  self: CharCombatant
  party: CharCombatant[]
  pcs: CharCombatant[]
  targets: CharCombatant[]
  zoneId: number
  zoneName: string
  countPC: number
  countNPC: number
  countChocobos: number
  countPets: number
  countTotal: number
  isCrowded: boolean
}

interface IFateEvent extends ICenturionEvent {
  state: string
  fateId: number
  progress: number
}

const DEFAULT_PLAYER: ICharacter = {
  jobId: 0,
  level: 0,
  worldID: 0,
  worldName: '',
  currentHp: 0,
  maxHp: 0,
  hpp: 0,
  partyType: 0,
  order: 0,
  posX: 0,
  posY: 0,
  posZ: 0,
  x: 0,
  y: 0,
  z: 0,
  heading: 0,
  distance: 0,
  id: 0,
  name: '',
  ownerId: 0,
  targetId: 0,
  bNpcId: 0,
  bNpcNameId: 0,
  type: 0
}

export { DEFAULT_PLAYER, toCharacter }
export type {
  ICharacter,
  ICombatDataEvent,
  IFateEvent,
  ILocationNotifiedEvent,
  IMobFAEvent,
  IMobLocationEvent,
  IMobStateChangedEvent,
  IMobTriggerEvent,
  IPlayerLoggedInEvent,
  IPlayerLoggedOutEvent,
  IZoneChangedEvent,
  IZoneInstanceChangedEvent
}
