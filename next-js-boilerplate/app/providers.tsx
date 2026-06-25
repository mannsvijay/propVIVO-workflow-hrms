'use client';

import { Provider as ReduxProvider } from "react-redux";
import { ApolloProvider } from "@apollo/client/react";
import { PropsWithChildren } from "react";
import { store } from "../store";
import { apolloClient } from "../lib/apolloClient";
import { SessionProvider } from "../context/SessionContext";

export function Providers({ children }: PropsWithChildren) {
	return (
		<ReduxProvider store={store}>
			<ApolloProvider client={apolloClient}>
				<SessionProvider>{children}</SessionProvider>
			</ApolloProvider>
		</ReduxProvider>
	);
}


