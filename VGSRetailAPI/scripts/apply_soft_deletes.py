import os
import re

def insert_before_last_brace(filepath, content_to_insert):
    with open(filepath, 'r') as f:
        content = f.read()
    if content_to_insert in content:
        return
    last_brace_idx = content.rfind('}')
    if last_brace_idx != -1:
        new_content = content[:last_brace_idx] + content_to_insert + content[last_brace_idx:]
        with open(filepath, 'w') as f:
            f.write(new_content)

def process_entity(name, lower_name, route_name):
    base_dir = '/Users/sauravmishra/VGSRetail/VGSRetailAPI/backend/src'
    
    # Update IDAC
    idac_path = next(os.path.join(r, f) for r, d, files in os.walk(base_dir) for f in files if f == f'I{name}DAC.cs')
    insert_before_last_brace(idac_path, f'    Task<bool> Delete{name}Async(Guid id, string tenantId, CancellationToken cancellationToken);\n')
    
    # Update DAC
    dac_path = next(os.path.join(r, f) for r, d, files in os.walk(base_dir) for f in files if f == f'{name}DAC.cs')
    dac_code = f"""
    public async Task<bool> Delete{name}Async(Guid id, string tenantId, CancellationToken cancellationToken)
    {{
        var entity = await _dbContext.{name}s
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
            
        if (entity == null) return false;
        
        _dbContext.{name}s.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }}
"""
    # Insert right before private map method if possible, else before last brace
    with open(dac_path, 'r') as f:
        content = f.read()
    if dac_code not in content:
        if 'private ' in content:
            idx = content.rfind('private ')
            content = content[:idx] + dac_code + content[idx:]
        else:
            idx = content.rfind('}')
            content = content[:idx] + dac_code + content[idx:]
        with open(dac_path, 'w') as f:
            f.write(content)
            
    # Update IBL
    ibl_path = next(os.path.join(r, f) for r, d, files in os.walk(base_dir) for f in files if f == f'I{name}BL.cs')
    insert_before_last_brace(ibl_path, f'    Task Delete{name}Async(Guid id, CancellationToken cancellationToken);\n')

    # Update BL
    bl_path = next(os.path.join(r, f) for r, d, files in os.walk(base_dir) for f in files if f == f'{name}BL.cs')
    bl_code = f"""
    public async Task Delete{name}Async(Guid id, CancellationToken cancellationToken)
    {{
        var tenantId = GetTenantId();
        var success = await _{lower_name}Dac.Delete{name}Async(id, tenantId, cancellationToken);
        if (!success)
            throw new NotFoundException($"'{name}' with ID {{id}} not found.");
    }}
"""
    with open(bl_path, 'r') as f:
        content = f.read()
    if bl_code not in content:
        if 'private ' in content:
            idx = content.rfind('private ')
            content = content[:idx] + bl_code + content[idx:]
        else:
            idx = content.rfind('}')
            content = content[:idx] + bl_code + content[idx:]
        with open(bl_path, 'w') as f:
            f.write(content)

    # Update Controller
    ctrl_path = next(os.path.join(r, f) for r, d, files in os.walk(base_dir) for f in files if f == f'{name}Controller.cs')
    ctrl_code = f"""
    [HttpDelete("{{id}}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete{name}(Guid id, CancellationToken cancellationToken)
    {{
        await _{lower_name}Bl.Delete{name}Async(id, cancellationToken);
        return NoContent();
    }}
"""
    insert_before_last_brace(ctrl_path, ctrl_code)
    print(f'Processed {name}')

process_entity('Customer', 'customer', 'customers')
process_entity('Supplier', 'supplier', 'suppliers')
