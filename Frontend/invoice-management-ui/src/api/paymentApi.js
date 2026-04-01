import axiosClient from "./axiosClient";

export const addPayment = async (invoiceId, paymentData) => {
  const response = await axiosClient.post(`/invoices/${invoiceId}/payments`, paymentData);
  return response.data;
};