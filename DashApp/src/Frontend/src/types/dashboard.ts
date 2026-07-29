export interface Tile {
  provider: string
  connected: boolean
  connectUrl: string | null
  connectedAccount: string | null
}

export interface DashboardResponse {
  tiles: Tile[]
}