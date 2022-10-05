export const mockStartMethod = vi.fn();
export const mockEndMethod = vi.fn();
export const mockIsMethod = vi.fn();
export const mockAnyMethod = vi.fn();

const mock = vi.fn().mockImplementation(() => {
  return {
    start: mockStartMethod,
    end: mockEndMethod,
    is: mockIsMethod,
    any: mockAnyMethod,
  };
});

export default mock;
