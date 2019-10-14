/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Server.web
{
	using Connection = org.eclipse.jetty.io.Connection;
	using Test = org.junit.jupiter.api.Test;

	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class JettyHttpConnectionListenerTest
	{
		private bool InstanceFieldsInitialized = false;

		public JettyHttpConnectionListenerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_listener = new JettyHttpConnectionListener( _connectionTracker );
		}

		 private readonly NetworkConnectionTracker _connectionTracker = mock( typeof( NetworkConnectionTracker ) );
		 private JettyHttpConnectionListener _listener;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotifyAboutOpenConnection()
		 internal virtual void ShouldNotifyAboutOpenConnection()
		 {
			  JettyHttpConnection connection = mock( typeof( JettyHttpConnection ) );

			  _listener.onOpened( connection );

			  verify( _connectionTracker ).add( connection );
			  verify( _connectionTracker, never() ).remove(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotifyAboutClosedConnection()
		 internal virtual void ShouldNotifyAboutClosedConnection()
		 {
			  JettyHttpConnection connection = mock( typeof( JettyHttpConnection ) );

			  _listener.onClosed( connection );

			  verify( _connectionTracker, never() ).add(any());
			  verify( _connectionTracker ).remove( connection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIgnoreOpenConnectionOfUnknownType()
		 internal virtual void ShouldIgnoreOpenConnectionOfUnknownType()
		 {
			  Connection connection = mock( typeof( Connection ) );

			  _listener.onOpened( connection );

			  verify( _connectionTracker, never() ).add(any());
			  verify( _connectionTracker, never() ).remove(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIgnoreClosedConnectionOfUnknownType()
		 internal virtual void ShouldIgnoreClosedConnectionOfUnknownType()
		 {
			  Connection connection = mock( typeof( Connection ) );

			  _listener.onClosed( connection );

			  verify( _connectionTracker, never() ).add(any());
			  verify( _connectionTracker, never() ).remove(any());
		 }
	}

}