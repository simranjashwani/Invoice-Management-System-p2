import { calculateLineTotal } from "../../utils/calculations";

function LineItemsForm({ lineItems, onChange, onAdd, onRemove }) {
  return (
    <div className="card">
      <div className="section-header">
        <h3>Line Items</h3>
        <button type="button" className="btn" onClick={onAdd}>
          Add Line Item
        </button>
      </div>

      {lineItems.map((item, index) => (
        <div className="line-item-grid" key={index}>
          <input
            name="description"
            placeholder="Description"
            value={item.description}
            onChange={(e) => onChange(index, e)}
          />
          <input
            name="quantity"
            type="number"
            min="1"
            placeholder="Qty"
            value={item.quantity}
            onChange={(e) => onChange(index, e)}
          />
          <input
            name="unitPrice"
            type="number"
            min="0"
            step="0.01"
            placeholder="Unit Price"
            value={item.unitPrice}
            onChange={(e) => onChange(index, e)}
          />
          <input
            name="discount"
            type="number"
            min="0"
            step="0.01"
            placeholder="Discount"
            value={item.discount}
            onChange={(e) => onChange(index, e)}
          />
          <input
            name="tax"
            type="number"
            min="0"
            step="0.01"
            placeholder="Tax"
            value={item.tax}
            onChange={(e) => onChange(index, e)}
          />

          <div className="line-total-box">
            <strong>Line Total:</strong> {calculateLineTotal(item).toFixed(2)}
          </div>

          {lineItems.length > 1 && (
            <button
              type="button"
              className="btn btn-danger"
              onClick={() => onRemove(index)}
            >
              Remove
            </button>
          )}
        </div>
      ))}
    </div>
  );
}

export default LineItemsForm;