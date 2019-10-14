using System;

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
namespace Neo4Net.Kernel.impl.query.clientconnection
{
	/// <summary>
	/// This is implemented as an abstract class in order to support different formatting for <seealso cref="asConnectionDetails()"/>,
	/// when this method is no longer needed, and we move to a standardized format across all types of connections, we can
	/// turn this class into a simpler value type that just holds the fields that are actually used.
	/// </summary>
	public abstract class ClientConnectionInfo
	{
		 /// <summary>
		 /// Used by <seealso cref="asConnectionDetails()"/> only. When the {@code connectionDetails} string is no longer needed,
		 /// this can go away, since the username is provided though other means to the places that need it.
		 /// </summary>
		 [Obsolete]
		 public virtual ClientConnectionInfo WithUsername( string username )
		 {
			  return new ConnectionInfoWithUsername( this, username );
		 }

		 /// <summary>
		 /// This method provides the custom format for each type of connection.
		 /// <para>
		 /// Preferably we would not need to have a custom format for each type of connection, but this is provided for
		 /// backwards compatibility reasons.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a custom log-line format describing this type of connection. </returns>
		 [Obsolete]
		 public abstract string AsConnectionDetails();

		 /// <summary>
		 /// Which protocol was used for this connection.
		 /// <para>
		 /// This is not necessarily an internet protocol (like http et.c.) although it could be. It might also be "embedded"
		 /// for example, if this connection represents an embedded session.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the protocol used for connecting to the server. </returns>
		 public abstract string Protocol();

		 /// <summary>
		 /// Identifier of the network connection.
		 /// </summary>
		 /// <returns> the identifier or {@code null} for embedded connections. </returns>
		 public abstract string ConnectionId();

		 /// <summary>
		 /// This method is overridden in the subclasses where this information is available.
		 /// </summary>
		 /// <returns> the address of the client. or {@code null} if the address is not available. </returns>
		 public virtual string ClientAddress()
		 {
			  return null;
		 }

		 /// <summary>
		 /// This method is overridden in the subclasses where this information is available.
		 /// </summary>
		 /// <returns> the URI of this server that the client connected to, or {@code null} if the URI is not available. </returns>
		 public virtual string RequestURI()
		 {
			  return null;
		 }

		 public static readonly ClientConnectionInfo EMBEDDED_CONNECTION = new ClientConnectionInfoAnonymousInnerClass();

		 private class ClientConnectionInfoAnonymousInnerClass : ClientConnectionInfo
		 {
			 public override string asConnectionDetails()
			 {
				  return "embedded-session\t";
			 }

			 public override string protocol()
			 {
				  return "embedded";
			 }

			 public override string connectionId()
			 {
				  return null;
			 }
		 }

		 /// <summary>
		 /// Should be removed along with <seealso cref="withUsername(string)"/> and <seealso cref="asConnectionDetails()"/>.
		 /// </summary>
		 [Obsolete]
		 private class ConnectionInfoWithUsername : ClientConnectionInfo
		 {
			  internal readonly ClientConnectionInfo Source;
			  internal readonly string Username;

			  internal ConnectionInfoWithUsername( ClientConnectionInfo source, string username )
			  {
					this.Source = source;
					this.Username = username;
			  }

			  public override string AsConnectionDetails()
			  {
					return Source.asConnectionDetails() + '\t' + Username;
			  }

			  public override string Protocol()
			  {
					return Source.protocol();
			  }

			  public override string ConnectionId()
			  {
					return Source.connectionId();
			  }

			  public override string ClientAddress()
			  {
					return Source.clientAddress();
			  }

			  public override string RequestURI()
			  {
					return Source.requestURI();
			  }
		 }
	}

}