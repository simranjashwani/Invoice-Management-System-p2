import axiosClient from "./axiosClient";

export const getAgingReport = async () => {
  const response = await axiosClient.get("/invoices/analytics/aging");
  return response.data;
};

export const getRevenueSummary = async () => {
  const response = await axiosClient.get("/invoices/analytics/revenue-summary");
  return response.data;
};

export const getDso = async () => {
  const response = await axiosClient.get("/invoices/analytics/dso");
  return response.data;
};

export const getOutstanding = async () => {
  const response = await axiosClient.get("/invoices/analytics/outstanding");
  return response.data;
};