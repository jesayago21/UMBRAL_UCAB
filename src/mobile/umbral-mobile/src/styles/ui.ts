import { StyleSheet } from 'react-native'

export const colors = {
  indigo: '#4f46e5',
  indigoDark: '#3730a3',
  slate50: '#f8fafc',
  slate100: '#f1f5f9',
  slate200: '#e2e8f0',
  slate500: '#64748b',
  slate600: '#475569',
  slate700: '#334155',
  slate900: '#0f172a',
  emerald: '#059669',
  amber: '#b45309',
  red: '#dc2626',
  white: '#ffffff',
}

export const styles = StyleSheet.create({
  screen: {
    flex: 1,
    backgroundColor: colors.slate50,
  },
  content: {
    padding: 16,
    gap: 12,
  },
  card: {
    backgroundColor: colors.white,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.slate200,
    padding: 16,
    gap: 8,
  },
  title: {
    fontSize: 22,
    fontWeight: '700',
    color: colors.slate900,
  },
  subtitle: {
    fontSize: 14,
    color: colors.slate600,
  },
  heading: {
    fontSize: 16,
    fontWeight: '600',
    color: colors.slate900,
  },
  body: {
    fontSize: 14,
    color: colors.slate700,
  },
  muted: {
    fontSize: 13,
    color: colors.slate500,
  },
  input: {
    borderWidth: 1,
    borderColor: colors.slate200,
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 10,
    fontSize: 15,
    color: colors.slate900,
    backgroundColor: colors.white,
  },
  btnPrimary: {
    backgroundColor: colors.indigo,
    borderRadius: 10,
    paddingVertical: 12,
    paddingHorizontal: 16,
    alignItems: 'center',
  },
  btnPrimaryText: {
    color: colors.white,
    fontWeight: '600',
    fontSize: 15,
  },
  btnSecondary: {
    borderWidth: 1,
    borderColor: colors.slate200,
    backgroundColor: colors.white,
    borderRadius: 10,
    paddingVertical: 12,
    paddingHorizontal: 16,
    alignItems: 'center',
  },
  btnSecondaryText: {
    color: colors.slate700,
    fontWeight: '600',
    fontSize: 15,
  },
  error: {
    color: colors.red,
    fontSize: 14,
  },
  success: {
    color: colors.emerald,
    fontSize: 14,
    fontWeight: '600',
  },
  rowBetween: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: 8,
  },
  chip: {
    alignSelf: 'flex-start',
    borderRadius: 999,
    paddingHorizontal: 10,
    paddingVertical: 4,
    backgroundColor: colors.slate100,
  },
  chipText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.slate700,
  },
})
