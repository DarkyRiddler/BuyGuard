'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import { useRouter } from 'next/navigation';

const formatDateForBackend = (dateString: string, isEndDate: boolean = false): string => {
  if (!dateString) return '';

  try {
    if (isEndDate) {
      return dateString + 'T23:59:59.999Z';
    } else {
      return dateString + 'T00:00:00.000Z';
    }
  } catch (error) {
    console.error('Error formatting date:', error);
    return '';
  }
};

export default function FiltrationCard() {
  const router = useRouter();

  const [filters, setFilters] = useState({
    status: '',
    minAmount: '',
    maxAmount: '',
    dateFrom: '',
    dateTo: '',
    searchName: '',
    sortBy: '',
    sortOrder: 'asc',
  });

  const statusOptions = [
    'czeka',
    'potwierdzono',
    'odrzucono',
    'zakupione',
  ];

  const sortOptions = [
    { value: 'none', label: 'Bez sortowania' },
    { value: 'amount', label: 'Kwota' },
    { value: 'createdat', label: 'Data utworzenia' },
    { value: 'status', label: 'Status' },
    { value: 'username', label: 'Nazwa użytkownika' },
  ];

  const sortOrderOptions = [
    { value: 'asc', label: 'Rosnąco' },
    { value: 'desc', label: 'Malejąco' },
  ];
  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({
      ...prev,
      [key]: value,
    }));
  };

  const applyFilters = () => {
    const params = new URLSearchParams();

    Object.entries(filters).forEach(([key, value]) => {
      if (value) {
        if (key === 'dateFrom') {
          params.set(key, formatDateForBackend(value, false));
        } else if (key === 'dateTo') {
          params.set(key, formatDateForBackend(value, true));
        } else {
          params.set(key, value);
        }
      }
    });

    router.push(`?${params.toString()}`);
  };

  const clearFilters = () => {
    setFilters({
      status: '',
      minAmount: '',
      maxAmount: '',
      dateFrom: '',
      dateTo: '',
      searchName: '',
      sortBy: '',
      sortOrder: 'asc',
    });
    router.push('?');
  };
  return (
    <Card className="mb-6 rounded-2xl">
      <CardHeader>
        <CardTitle className="text-lg">Filtry i sortowanie</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 mb-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="status">Status</Label>
            <Select value={filters.status || 'all'}
                    onValueChange={(value) => handleFilterChange('status', value === 'all' ? '' : value)}>
              <SelectTrigger>
                <SelectValue placeholder="Wszystkie"/>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Wszystkie</SelectItem>
                {statusOptions.map(status => (
                  <SelectItem key={status} value={status}>
                    {status === 'czeka' && 'Czeka'}
                    {status === 'potwierdzono' && 'Potwierdzone'}
                    {status === 'odrzucono' && 'Odrzucone'}
                    {status === 'zakupione' && 'Zakupione'}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="minAmount">Minimalna kwota (PLN)</Label>
            <Input
              id="minAmount"
              type="number"
              step="0.01"
              value={filters.minAmount}
              onChange={(e) => handleFilterChange('minAmount', e.target.value)}
              placeholder="0.00"
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="maxAmount">Maksymalna kwota (PLN)</Label>
            <Input
              id="maxAmount"
              type="number"
              step="0.01"
              value={filters.maxAmount}
              onChange={(e) => handleFilterChange('maxAmount', e.target.value)}
              placeholder="10000.00"
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="dateFrom">Data od</Label>
            <Input
              id="dateFrom"
              type="date"
              value={filters.dateFrom}
              onChange={(e) => handleFilterChange('dateFrom', e.target.value)}
            />

          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="dateTo">Data do</Label>
            <Input
              id="dateTo"
              type="date"
              value={filters.dateTo}
              onChange={(e) => handleFilterChange('dateTo', e.target.value)}
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="searchName">Szukaj po nazwie</Label>
            <Input
              id="searchName"
              type="text"
              value={filters.searchName}
              onChange={(e) => handleFilterChange('searchName', e.target.value)}
              placeholder="Imię, nazwisko lub email"
            />
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="sortBy">Sortuj po</Label>
            <Select value={filters.sortBy || 'none'}
                    onValueChange={(value) => handleFilterChange('sortBy', value === 'none' ? '' : value)}>
              <SelectTrigger>
                <SelectValue placeholder="Bez sortowania"/>
              </SelectTrigger>
              <SelectContent>
                {sortOptions.map(option => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="sortOrder">Kierunek sortowania</Label>
            <Select value={filters.sortOrder} onValueChange={(value) => handleFilterChange('sortOrder', value)}>
              <SelectTrigger>
                <SelectValue/>
              </SelectTrigger>
              <SelectContent>
                {sortOrderOptions.map(option => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className="flex items-center justify-center gap-2">
            <Button onClick={applyFilters}>
              Zastosuj filtry
            </Button>
            <Button onClick={clearFilters} variant="outline">
              Wyczyść filtry
            </Button>
          </div>
        </div>

      </CardContent>
    </Card>
  );
}