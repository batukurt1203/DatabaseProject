INSERT INTO SupportTicket (TicketID, ProductID, CustomerID, Employee_PersonID, DateOpened, DateClosed, Status, IssueDescription)
VALUES
    (1, 1, 1, 2, '2024-01-10', NULL, 'Open', 'Login screen crashes after update.'),
-- IRONIC ürünü için Giriş ekranı hatası (Henüz kapanmamış)

    (2, 2, 2, 1, '2024-01-12', '2024-01-13', 'Resolved', 'License key activation error.'),
-- ISGPRO ürünü için Lisans hatası (Çözülmüş)

    (3, 1, 3, 2, '2024-01-15', NULL, 'In Progress', 'Data export to PDF feature is not working properly.');
-- IRONIC ürünü için PDF çıktı hatası (İşlemde)