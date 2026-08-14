import { Box, CircularProgress, Typography } from "@mui/material";

export default function PageLoader({ label = "جاري التحميل..." }) {
  return (
    <Box className="page-loader" role="status" aria-live="polite">
      <CircularProgress size={34} thickness={4} />
      <Typography component="span">{label}</Typography>
    </Box>
  );
}
