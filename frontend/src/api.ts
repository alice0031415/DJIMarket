export type RankBy = 'profit' | 'averageCheck';

export interface DashboardResponse {
  from: string;
  to: string;
  previousFrom: string;
  previousTo: string;
  kpis: {
    revenue: number;
    grossProfit: number;
    margin: number;
    salesCount: number;
    averageCheck: number;
    bestManager: string | null;
    revenueChangePct: number;
    profitChangePct: number;
    salesCountChangePct: number;
    averageCheckChangePct: number;
  };
  managerRanking: Array<{
    rank: number;
    managerId: string;
    manager: string;
    team: string;
    salesCount: number;
    revenue: number;
    grossProfit: number;
    averageCheck: number;
    margin: number;
    changePct: number;
  }>;
  trend: Array<{ date: string; revenue: number; grossProfit: number; salesCount: number }>;
  categories: Array<{ category: string; revenue: number; grossProfit: number; sharePct: number }>;
  topProducts: Array<{ product: string; category: string; units: number; revenue: number; grossProfit: number }>;
  recentSales: Array<{ date: string; manager: string; customer: string; products: string; status: string; amount: number; grossProfit: number }>;
}

export async function fetchDashboard(from: string, to: string, rankBy: RankBy, signal?: AbortSignal) {
  const params = new URLSearchParams({ from, to, rankBy });
  const response = await fetch(`/api/dashboard?${params}`, { signal });
  if (!response.ok) {
    const body = await response.json().catch(() => null) as { message?: string } | null;
    throw new Error(body?.message ?? `API error ${response.status}`);
  }
  return response.json() as Promise<DashboardResponse>;
}
