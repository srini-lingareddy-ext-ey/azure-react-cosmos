import {
  createContext,
  FC,
  PropsWithChildren,
  useCallback,
  useContext,
  useEffect,
  useState,
} from 'react';
import { MessageBar, MessageBarType, Stack } from '@fluentui/react';

type ToastState = { message: string; type: MessageBarType } | null;

const ToastContext = createContext<
  (message: string, type?: MessageBarType) => void
>(() => {});

const TOAST_DURATION_MS = 5000;

export const ToastProvider: FC<PropsWithChildren> = ({ children }) => {
  const [toast, setToast] = useState<ToastState>(null);

  const showToast = useCallback(
    (message: string, type: MessageBarType = MessageBarType.success) => {
      setToast({ message, type });
    },
    []
  );

  useEffect(() => {
    if (!toast) return;
    const id = window.setTimeout(() => setToast(null), TOAST_DURATION_MS);
    return () => clearTimeout(id);
  }, [toast]);

  return (
    <ToastContext.Provider value={showToast}>
      {children}
      {toast ? (
        <Stack
          styles={{
            root: {
              position: 'fixed',
              top: 16,
              right: 16,
              zIndex: 1000000,
              maxWidth: 440,
            },
          }}
        >
          <MessageBar
            messageBarType={toast.type}
            onDismiss={() => setToast(null)}
          >
            {toast.message}
          </MessageBar>
        </Stack>
      ) : null}
    </ToastContext.Provider>
  );
};

/** @see ToastProvider — hook must live with context for HMR; paired export. */
// eslint-disable-next-line react-refresh/only-export-components -- useToast is the consumer API for ToastProvider
export function useToast(): (message: string, type?: MessageBarType) => void {
  return useContext(ToastContext);
}
