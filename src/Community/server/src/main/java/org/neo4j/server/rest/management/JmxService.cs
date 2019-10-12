using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest.management
{

	using Kernel = Neo4Net.Jmx.Kernel;
	using JmxKernelExtension = Neo4Net.Jmx.impl.JmxKernelExtension;
	using Database = Neo4Net.Server.database.Database;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using JmxDomainRepresentation = Neo4Net.Server.rest.management.repr.JmxDomainRepresentation;
	using JmxMBeanRepresentation = Neo4Net.Server.rest.management.repr.JmxMBeanRepresentation;
	using ServiceDefinitionRepresentation = Neo4Net.Server.rest.management.repr.ServiceDefinitionRepresentation;
	using InputFormat = Neo4Net.Server.rest.repr.InputFormat;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using OutputFormat = Neo4Net.Server.rest.repr.OutputFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Path(JmxService.ROOT_PATH) public class JmxService implements AdvertisableService
	public class JmxService : AdvertisableService
	{
		 public const string ROOT_PATH = "server/jmx";

		 public const string DOMAINS_PATH = "/domain";
		 public static readonly string DomainTemplate = DOMAINS_PATH + "/{domain}";
		 public static readonly string BeanTemplate = DomainTemplate + "/{objectName}";
		 public const string QUERY_PATH = "/query";
		 public const string KERNEL_NAME_PATH = "/kernelquery";
		 private readonly OutputFormat _output;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public JmxService(@Context OutputFormat output, @Context InputFormat input)
		 public JmxService( OutputFormat output, InputFormat input )
		 {
			  this._output = output;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET public javax.ws.rs.core.Response getServiceDefinition()
		 public virtual Response ServiceDefinition
		 {
			 get
			 {
				  ServiceDefinitionRepresentation serviceDef = new ServiceDefinitionRepresentation( ROOT_PATH );
				  serviceDef.ResourceUri( "domains", JmxService.DOMAINS_PATH );
				  serviceDef.ResourceTemplate( "domain", JmxService.DomainTemplate );
				  serviceDef.ResourceTemplate( "bean", JmxService.BeanTemplate );
				  serviceDef.ResourceUri( "query", JmxService.QUERY_PATH );
				  serviceDef.ResourceUri( "kernelquery", JmxService.KERNEL_NAME_PATH );
   
				  return _output.ok( serviceDef );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(DOMAINS_PATH) public javax.ws.rs.core.Response listDomains() throws NullPointerException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual Response ListDomains()
		 {
			  MBeanServer server = ManagementFactory.PlatformMBeanServer;
			  ListRepresentation domains = ListRepresentation.strings( server.Domains );
			  return _output.ok( domains );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(DOMAIN_TEMPLATE) public javax.ws.rs.core.Response getDomain(@PathParam("domain") String domainName)
		 public virtual Response GetDomain( string domainName )
		 {
			  MBeanServer server = ManagementFactory.PlatformMBeanServer;

			  JmxDomainRepresentation domain = new JmxDomainRepresentation( domainName );

			  foreach ( object objName in server.queryNames( null, null ) )
			  {
					if ( objName.ToString().StartsWith(domainName, StringComparison.Ordinal) )
					{
						 domain.AddBean( ( ObjectName ) objName );
					}
			  }

			  return _output.ok( domain );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Path(BEAN_TEMPLATE) public javax.ws.rs.core.Response getBean(@PathParam("domain") String domainName, @PathParam("objectName") String objectName)
		 public virtual Response GetBean( string domainName, string objectName )
		 {
			  MBeanServer server = ManagementFactory.PlatformMBeanServer;

			  List<JmxMBeanRepresentation> beans = new List<JmxMBeanRepresentation>();
			  foreach ( object objName in server.queryNames( CreateObjectName( domainName, objectName ), null ) )
			  {
					beans.Add( new JmxMBeanRepresentation( ( ObjectName ) objName ) );
			  }

			  return _output.ok( new ListRepresentation( "bean", beans ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private javax.management.ObjectName createObjectName(final String domainName, final String objectName)
		 private ObjectName CreateObjectName( string domainName, string objectName )
		 {
			  try
			  {
					return new ObjectName( domainName + ":" + URLDecoder.decode( objectName, StandardCharsets.UTF_8.name() ) );
			  }
			  catch ( Exception e ) when ( e is MalformedObjectNameException || e is UnsupportedEncodingException )
			  {
					throw new WebApplicationException( e, 400 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Consumes(javax.ws.rs.core.MediaType.APPLICATION_JSON) @Path(QUERY_PATH) @SuppressWarnings("unchecked") public javax.ws.rs.core.Response queryBeans(String query)
		 public virtual Response QueryBeans( string query )
		 {
			  try
			  {
					MBeanServer server = ManagementFactory.PlatformMBeanServer;

					string json = DodgeStartingUnicodeMarker( query );
					ICollection<object> queries = ( ICollection<object> ) JsonHelper.readJson( json );

					List<JmxMBeanRepresentation> beans = new List<JmxMBeanRepresentation>();
					foreach ( object queryObj in queries )
					{
						 Debug.Assert( queryObj is string );
						 foreach ( object objName in server.queryNames( new ObjectName( ( string ) queryObj ), null ) )
						 {
							  beans.Add( new JmxMBeanRepresentation( ( ObjectName ) objName ) );
						 }
					}

					return _output.ok( new ListRepresentation( "jmxBean", beans ) );
			  }
			  catch ( Exception e ) when ( e is JsonParseException || e is MalformedObjectNameException )
			  {
					return _output.badRequest( e );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @POST @Produces(javax.ws.rs.core.MediaType.APPLICATION_JSON) @Consumes(javax.ws.rs.core.MediaType.APPLICATION_FORM_URLENCODED) @Path(QUERY_PATH) public javax.ws.rs.core.Response formQueryBeans(@FormParam("value") String data)
		 public virtual Response FormQueryBeans( string data )
		 {
			  return QueryBeans( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @GET @Produces(javax.ws.rs.core.MediaType.APPLICATION_JSON) @Path(KERNEL_NAME_PATH) public javax.ws.rs.core.Response currentKernelInstance(@Context Database database)
		 public virtual Response CurrentKernelInstance( Database database )
		 {
			  Kernel kernelBean = database.Graph.DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( Kernel ) );
			  return Response.ok( "\"" + kernelBean.MBeanQuery.ToString() + "\"" ).type(MediaType.APPLICATION_JSON).build();
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return "jmx";
			 }
		 }

		 public virtual string ServerPath
		 {
			 get
			 {
				  return ROOT_PATH;
			 }
		 }

		 private static string DodgeStartingUnicodeMarker( string @string )
		 {
			  if ( !string.ReferenceEquals( @string, null ) && @string.Length > 0 )
			  {
					if ( @string[0] == ( char )0xfeff )
					{
						 return @string.Substring( 1 );
					}
			  }
			  return @string;
		 }
	}

}