import BoltIcon from '@mui/icons-material/Bolt';
import { useNavigate } from 'react-router-dom';
import KPICard from '@/components/dashboard/KPICard';
import { useYieldSummary } from '@/hooks/useYieldReports';
import { formatCurrency, formatNumber } from '@/utils/formatters';

export default function YieldRoiCard({ targetPath }) {
  const navigate = useNavigate();
  const { data, isLoading } = useYieldSummary();
  const summary = data?.data;

  const roiValue = isLoading || !summary
    ? '...'
    : `${Number(summary.roiPercent || 0).toFixed(2)}%`;

  const subtitle = isLoading || !summary
    ? 'Loading today\'s yield efficiency'
    : `Yield ${Number(summary.yieldPercent || 0).toFixed(2)}% | Net ${formatNumber(summary.totalNetProductionSFT)} SFT | Cost/SFT ${formatCurrency(summary.costPerSFT)}`;

  const color = !summary
    ? 'primary.main'
    : summary.roiPercent >= 0
      ? 'success.main'
      : 'error.main';

  return (
    <KPICard
      title="Today's Yield ROI"
      value={roiValue}
      subtitle={subtitle}
      icon={<BoltIcon fontSize="inherit" />}
      color={color}
      onClick={() => navigate(targetPath)}
    />
  );
}