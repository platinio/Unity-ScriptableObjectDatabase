import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

// Without Vitest's globals, Testing Library can't remove what one test drew before the next.
afterEach(cleanup);
