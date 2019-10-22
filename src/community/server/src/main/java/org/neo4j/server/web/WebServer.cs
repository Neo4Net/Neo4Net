using System.Collections.Generic;

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
	using RequestLog = org.eclipse.jetty.server.RequestLog;
	using Server = org.eclipse.jetty.server.Server;


	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Neo4Net.Server.database;
	using Neo4Net.Server.plugins;
	using SslPolicy = Neo4Net.Ssl.SslPolicy;

	public interface WebServer
	{
		 ListenSocketAddress HttpAddress { set; }

		 ListenSocketAddress HttpsAddress { set; }

		 SslPolicy SslPolicy { set; }

		 RequestLog RequestLog { set; }

		 int MaxThreads { set; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void start() throws Exception;
		 void Start();

		 void Stop();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void addJAXRSPackages(java.util.List<String> packageNames, String serverMountPoint, java.util.Collection<org.Neo4Net.server.plugins.Injectable<?>> injectables);
		 void addJAXRSPackages<T1>( IList<string> packageNames, string serverMountPoint, ICollection<T1> injectables );
		 void RemoveJAXRSPackages( IList<string> packageNames, string serverMountPoint );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void addJAXRSClasses(java.util.List<String> classNames, String serverMountPoint, java.util.Collection<org.Neo4Net.server.plugins.Injectable<?>> injectables);
		 void addJAXRSClasses<T1>( IList<string> classNames, string serverMountPoint, ICollection<T1> injectables );
		 void RemoveJAXRSClasses( IList<string> classNames, string serverMountPoint );

		 void AddFilter( Filter filter, string pathSpec );

		 void RemoveFilter( Filter filter, string pathSpec );

		 void AddStaticContent( string contentLocation, string serverMountPoint );
		 void RemoveStaticContent( string contentLocation, string serverMountPoint );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void invokeDirectly(String targetUri, javax.servlet.http.HttpServletRequest request, javax.servlet.http.HttpServletResponse response) throws java.io.IOException, javax.servlet.ServletException;
		 void InvokeDirectly( string targetUri, HttpServletRequest request, HttpServletResponse response );

		 bool WadlEnabled { set; }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: void setDefaultInjectables(java.util.Collection<org.Neo4Net.server.database.InjectableProvider<?>> defaultInjectables);
		 ICollection<T1> DefaultInjectables<T1> { set; }

		 System.Action<Server> JettyCreatedCallback { set; }

		 /// <returns> local http connector bind port </returns>
		 InetSocketAddress LocalHttpAddress { get; }

		 /// <returns> local https connector bind port </returns>
		 InetSocketAddress LocalHttpsAddress { get; }
	}

}