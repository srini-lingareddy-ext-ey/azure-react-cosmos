export interface QueryTemplateResponse { id: string; tenantId: string; templateName: string; connectorTypeId: string; templateBody: string; parameters: string[]; isActive: boolean; createdAt?: string; updatedAt?: string; }
export interface QueryTemplateListResponse { items: QueryTemplateResponse[]; totalCount: number; }
export interface CreateQueryTemplateRequest { templateName: string; connectorTypeId: string; templateBody: string; parameters?: string[]; }
export interface UpdateQueryTemplateRequest { templateName?: string; templateBody?: string; parameters?: string[]; propagationMode?: 'allExisting' | 'newOnly'; }
