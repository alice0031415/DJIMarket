import { useMemo, useState } from 'react';
import { Alert, Avatar, Badge, Button, Card, ConfigProvider, DatePicker, Empty, Progress, Segmented, Spin, Table, Tag } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';
import { useQuery } from '@tanstack/react-query';
import { Area, Bar, CartesianGrid, ComposedChart, Legend, Line, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { fetchDashboard } from './api';
import type { RankBy } from './api';
import './styles.css';

const money = new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB', maximumFractionDigits: 0 });
const compact = new Intl.NumberFormat('ru-RU', { notation: 'compact', maximumFractionDigits: 1 });
const pct = new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 1 });
const fmtMoney = (v: number) => money.format(v).replace('RUB', '₽');
const fmtCompact = (v: number) => compact.format(v).replace(' ', ' ');

function initials(name: string) { return name.split(' ').map(x => x[0]).slice(0, 2).join(''); }
function iso(d: Dayjs) { return d.format('YYYY-MM-DD'); }

const today = dayjs('2026-09-24');

type PeriodKey = 'today' | '7d' | '30d' | 'thisMonth' | 'lastMonth' | 'custom';

export default function App() {
  const [period, setPeriod] = useState<PeriodKey>('30d');
  const [custom, setCustom] = useState<[Dayjs, Dayjs] | null>(null);
  const [rankBy, setRankBy] = useState<RankBy>('profit');

  const range = useMemo(() => {
    if (period === 'today') return [today, today.add(1, 'day')] as const;
    if (period === '7d') return [today.subtract(6, 'day'), today.add(1, 'day')] as const;
    if (period === 'thisMonth') return [today.startOf('month'), today.add(1, 'day')] as const;
    if (period === 'lastMonth') return [today.subtract(1, 'month').startOf('month'), today.startOf('month')] as const;
    if (period === 'custom' && custom) return [custom[0], custom[1].add(1, 'day')] as const;
    return [today.subtract(29, 'day'), today.add(1, 'day')] as const;
  }, [period, custom]);

  const query = useQuery({
    queryKey: ['dashboard', iso(range[0]), iso(range[1]), rankBy],
    queryFn: ({ signal }) => fetchDashboard(iso(range[0]), iso(range[1]), rankBy, signal),
    staleTime: 30_000,
  });

  const data = query.data;
  const revenueSeries = data?.trend.map(x => ({ ...x, label: dayjs(x.date).format('DD.MM') })) ?? [];
  const categoryMax = Math.max(...(data?.categories.map(x => x.revenue) ?? [1]));

  const rankingColumns: ColumnsType<NonNullable<typeof data>['managerRanking'][number]> = [
    { title: '#', dataIndex: 'rank', width: 48, render: (v) => <span className="rankNo">{v}</span> },
    { title: 'Менеджер', dataIndex: 'manager', width: 220, render: (_, row) => <div className="managerCell"><Avatar size={34}>{initials(row.manager)}</Avatar><div><b>{row.manager}</b><small>{row.team}</small></div></div> },
    { title: 'Продажи', dataIndex: 'salesCount', align: 'right' },
    { title: 'Выручка', dataIndex: 'revenue', align: 'right', render: v => fmtMoney(v) },
    { title: 'Gross Profit', dataIndex: 'grossProfit', align: 'right', render: v => <b>{fmtMoney(v)}</b> },
    { title: 'Средний чек', dataIndex: 'averageCheck', align: 'right', render: v => fmtMoney(v) },
    { title: 'Маржа', dataIndex: 'margin', align: 'right', render: v => `${pct.format(v * 100)}%` },
    { title: 'Δ', dataIndex: 'changePct', align: 'right', render: v => <span className={v >= 0 ? 'positive' : 'negative'}>{v >= 0 ? '+' : ''}{pct.format(v)}%</span> },
  ];

  return (
    <ConfigProvider theme={{ token: { colorPrimary: '#2457e6', borderRadius: 12, fontFamily: 'Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif' }, components: { Card: { colorBorderSecondary: '#e8ecf4' } } }}>
      <div className="appShell">
        <header className="topbar">
          <div className="brand"><div className="brandMark">D</div><div><div className="brandName">DJI-Market</div><div className="brandSub">Sales Performance Dashboard</div></div></div>
          <div className="topbarMeta"><Badge status="processing" text="Live analytics" />{query.isFetching && <span className="updated">Updating…</span>}<span className="updated">Data refreshed just now</span><Avatar size={38} className="userAvatar">AM</Avatar></div>
        </header>

        <main className="content">
          <section className="heroRow">
            <div><div className="eyebrow">SALES OVERVIEW</div><h1>Показатели отдела продаж</h1><p>Сводка по выручке, прибыльности и эффективности менеджеров.</p></div>
            <div className="periodBar">
              <Segmented value={period} onChange={v => setPeriod(v as PeriodKey)} options={[{ label: 'Сегодня', value: 'today' }, { label: '7 дней', value: '7d' }, { label: '30 дней', value: '30d' }, { label: 'Этот месяц', value: 'thisMonth' }, { label: 'Прошлый месяц', value: 'lastMonth' }, { label: 'Период', value: 'custom' }]} />
              {period === 'custom' && <DatePicker.RangePicker value={custom ?? [today.subtract(29, 'day'), today]} onChange={v => setCustom(v as [Dayjs, Dayjs])} allowClear={false} format="DD.MM.YYYY" />}
            </div>
          </section>

          {query.isError && <Alert type="error" showIcon message="Не удалось загрузить dashboard" description={query.error instanceof Error ? query.error.message : 'Попробуйте ещё раз.'} action={<Button onClick={() => query.refetch()}>Повторить</Button>} className="pageAlert" />}

          <section className="kpiGrid">
            <Kpi title="Выручка" value={data ? fmtCompact(data.kpis.revenue) : '—'} exact={data ? fmtMoney(data.kpis.revenue) : undefined} change={data?.kpis.revenueChangePct} />
            <Kpi title="Gross Profit" value={data ? fmtCompact(data.kpis.grossProfit) : '—'} exact={data ? fmtMoney(data.kpis.grossProfit) : undefined} change={data?.kpis.profitChangePct} accent />
            <Kpi title="Маржинальность" value={data ? `${pct.format(data.kpis.margin * 100)}%` : '—'} change={data?.kpis.profitChangePct} />
            <Kpi title="Продажи" value={data ? compact.format(data.kpis.salesCount) : '—'} change={data?.kpis.salesCountChangePct} />
            <Kpi title="Средний чек" value={data ? fmtCompact(data.kpis.averageCheck) : '—'} exact={data ? fmtMoney(data.kpis.averageCheck) : undefined} change={data?.kpis.averageCheckChangePct} />
            <Card className="kpiCard winnerCard"><div className="kpiLabel">Лучший менеджер</div><div className="winner"><Avatar size={40}>{data?.kpis.bestManager ? initials(data.kpis.bestManager) : '—'}</Avatar><div><strong>{data?.kpis.bestManager ?? '—'}</strong><small>по Gross Profit</small></div></div></Card>
          </section>

          {query.isLoading && !data ? <div className="loadingPage"><Spin size="large" /><span>Загружаем аналитику…</span></div> : data ? <>
            <section className="mainGrid">
              <Card className="panel chartPanel" title={<SectionTitle title="Динамика продаж" hint={`${data.from} — ${data.to}`} />} extra={<Tag>Daily</Tag>}>
                {revenueSeries.length ? <ResponsiveContainer width="100%" height={310}><ComposedChart data={revenueSeries} margin={{ left: 8, right: 8, top: 15, bottom: 4 }}>
                  <defs><linearGradient id="revFill" x1="0" y1="0" x2="0" y2="1"><stop offset="0%" stopOpacity={0.28}/><stop offset="100%" stopOpacity={0}/></linearGradient></defs>
                  <CartesianGrid vertical={false} strokeDasharray="3 4" />
                  <XAxis dataKey="label" tickLine={false} axisLine={false} minTickGap={24} />
                  <YAxis yAxisId="left" tickLine={false} axisLine={false} tickFormatter={v => `${fmtCompact(Number(v))}`} width={58} />
                  <YAxis yAxisId="right" orientation="right" tickLine={false} axisLine={false} width={44} />
                  <Tooltip formatter={(v, name) => [name === 'salesCount' ? v : fmtMoney(Number(v)), name === 'revenue' ? 'Revenue' : name === 'grossProfit' ? 'Gross Profit' : 'Продажи']} />
                  <Legend iconType="circle" />
                  <Area yAxisId="left" type="monotone" dataKey="revenue" strokeWidth={2.4} fill="url(#revFill)" name="revenue" />
                  <Bar yAxisId="left" dataKey="grossProfit" barSize={8} radius={[5, 5, 0, 0]} name="grossProfit" />
                  <Line yAxisId="right" type="monotone" dataKey="salesCount" strokeWidth={2} dot={false} name="salesCount" />
                </ComposedChart></ResponsiveContainer> : <Empty description="Нет продаж за период" />}
              </Card>
              <Card className="panel categoryPanel" title={<SectionTitle title="Категории" hint="Доля выручки" />}>
                {data.categories.length ? <div className="categoryList">{data.categories.map(x => <div className="categoryItem" key={x.category}><div className="categoryTop"><span>{x.category}</span><b>{pct.format(x.sharePct * 100)}%</b></div><Progress percent={x.sharePct * 100} showInfo={false} strokeWidth={9} /><small>{fmtMoney(x.revenue)} · profit {fmtMoney(x.grossProfit)}</small></div>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Нет продаж за период" />}
              </Card>
            </section>

            <section className="mainGrid secondRow">
              <Card className="panel rankingPanel" title={<SectionTitle title="Рейтинг менеджеров" hint="Период vs previous comparable period" />} extra={<Segmented size="small" value={rankBy} onChange={v => setRankBy(v as RankBy)} options={[{ label: 'Gross Profit', value: 'profit' }, { label: 'Average Check', value: 'averageCheck' }]} />}>
                <Table size="middle" rowKey="managerId" columns={rankingColumns} dataSource={data.managerRanking} pagination={{ pageSize: 8, hideOnSinglePage: true }} scroll={{ x: 980 }} />
              </Card>
              <div className="stacked">
                <Card className="panel productsPanel" title={<SectionTitle title="Лучшие продукты" hint="по gross profit" />}>
                  {data.topProducts.length ? data.topProducts.map((x, idx) => <div className="productItem" key={x.product}><div className="productIcon">{idx + 1}</div><div className="productInfo"><b>{x.product}</b><small>{x.category} · {x.units} шт.</small></div><strong>{fmtCompact(x.grossProfit)}</strong></div>) : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Нет данных" />}
                </Card>
                <Card className="panel recentPanel" title={<SectionTitle title="Последние продажи" />}>
                  {data.recentSales.length ? <div className="recentList">{data.recentSales.slice(0, 6).map(x => <div className="recentItem" key={`${x.date}-${x.customer}`}><div><b>{x.customer}</b><small>{x.manager} · {dayjs(x.date).format('DD.MM HH:mm')}</small><small className="recentProducts">{x.products}</small></div><div className="recentRight"><Tag color={x.status === 'Paid' ? 'green' : x.status === 'Refunded' ? 'orange' : 'red'}>{x.status}</Tag><div><strong>{fmtMoney(x.amount)}</strong><small className="recentProfit">GP {fmtMoney(x.grossProfit)}</small></div></div></div>)}</div> : <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Нет продаж за период" />}
                </Card>
              </div>
            </section>
          </> : null}
        </main>
      </div>
    </ConfigProvider>
  );
}

function Kpi({ title, value, exact, change, accent }: { title: string; value: string; exact?: string; change?: number; accent?: boolean }) {
  return <Card className={`kpiCard ${accent ? 'accentKpi' : ''}`}><div className="kpiLabel">{title}</div><div className="kpiValue" title={exact}>{value}</div>{change !== undefined && <div className={change >= 0 ? 'delta positive' : 'delta negative'}>{change >= 0 ? '↑' : '↓'} {Math.abs(change).toFixed(1)}% <span>vs previous</span></div>}</Card>;
}

function SectionTitle({ title, hint }: { title: string; hint?: string }) { return <div><div className="sectionTitle">{title}</div>{hint && <div className="sectionHint">{hint}</div>}</div>; }
