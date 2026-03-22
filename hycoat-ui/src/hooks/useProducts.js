import { useState, useEffect } from 'react';
import { productService } from '../services/api';

export function useProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const data = await productService.getAll();
      setProducts(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const createProduct = async (product) => {
    const newProduct = await productService.create(product);
    setProducts(prev => [...prev, newProduct]);
    return newProduct;
  };

  const deleteProduct = async (id) => {
    await productService.delete(id);
    setProducts(prev => prev.filter(p => p.id !== id));
  };

  return { products, loading, error, refetch: fetchProducts, createProduct, deleteProduct };
}
