using System.Collections.Generic;
using System.Text;

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
	using ClassNamesResourceConfig = com.sun.jersey.api.core.ClassNamesResourceConfig;
	using PackagesResourceConfig = com.sun.jersey.api.core.PackagesResourceConfig;
	using ResourceConfig = com.sun.jersey.api.core.ResourceConfig;
	using ServletContainer = com.sun.jersey.spi.container.servlet.ServletContainer;
	using ServletHolder = org.eclipse.jetty.servlet.ServletHolder;


	using Neo4Net.Server.database;
	using ServerModule = Neo4Net.Server.modules.ServerModule;
	using Neo4Net.Server.plugins;

	/// <summary>
	/// Different <seealso cref="ServerModule"/>s can register services at the same mount point.
	/// So this class will collect all packages/classes per mount point and create the <seealso cref="ServletHolder"/>
	/// when all modules have registered services, see <seealso cref="create(System.Collections.ICollection, bool)"/>.
	/// </summary>
	public abstract class JaxRsServletHolderFactory
	{
		 private readonly IList<string> _items = new List<string>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.List<org.Neo4Net.server.plugins.Injectable<?>> injectables = new java.util.ArrayList<>();
		 private readonly IList<Injectable<object>> _injectables = new List<Injectable<object>>();

		 public virtual void Add<T1>( IList<string> items, ICollection<T1> injectableProviders )
		 {
			  ( ( IList<string> )this._items ).AddRange( items );
			  if ( injectableProviders != null )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: this.injectables.addAll(injectableProviders);
					( ( IList<Injectable<object>> )this._injectables ).AddRange( injectableProviders );
			  }
		 }

		 public virtual void Remove( IList<string> items )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			  this._items.removeAll( items );
		 }

		 public virtual ServletHolder Create<T1>( ICollection<T1> defaultInjectables, bool wadlEnabled )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.Neo4Net.server.database.InjectableProvider<?>> injectableProviders = mergeInjectables(defaultInjectables, injectables);
			  ICollection<InjectableProvider<object>> injectableProviders = MergeInjectables( defaultInjectables, _injectables );
			  ServletContainer container = new NeoServletContainer( injectableProviders );
			  ServletHolder servletHolder = new ServletHolder( container );
			  servletHolder.setInitParameter( ResourceConfig.FEATURE_DISABLE_WADL, ( !wadlEnabled ).ToString() );
			  Configure( servletHolder, ToCommaSeparatedList( _items ) );
			  servletHolder.setInitParameter( ResourceConfig.PROPERTY_CONTAINER_REQUEST_FILTERS, RequestFilterConfig );
			  return servletHolder;
		 }

		 private string RequestFilterConfig
		 {
			 get
			 {
				  // Ordering of execution of filters goes from left to right
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  return typeof( XForwardFilter ).FullName;
			 }
		 }

		 protected internal abstract void Configure( ServletHolder servletHolder, string commaSeparatedList );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Collection<org.Neo4Net.server.database.InjectableProvider<?>> mergeInjectables(java.util.Collection<org.Neo4Net.server.database.InjectableProvider<?>> defaultInjectables, java.util.Collection<org.Neo4Net.server.plugins.Injectable<?>> injectables)
		 private ICollection<InjectableProvider<object>> MergeInjectables<T1, T2>( ICollection<T1> defaultInjectables, ICollection<T2> injectables )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Collection<org.Neo4Net.server.database.InjectableProvider<?>> injectableProviders = new java.util.ArrayList<>();
			  ICollection<InjectableProvider<object>> injectableProviders = new List<InjectableProvider<object>>();
			  if ( defaultInjectables != null )
			  {
					injectableProviders.addAll( defaultInjectables );
			  }
			  if ( injectables != null )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.server.plugins.Injectable<?> injectable : injectables)
					foreach ( Injectable<object> injectable in injectables )
					{
						 injectableProviders.Add( new InjectableWrapper( injectable ) );
					}
			  }
			  return injectableProviders;
		 }

		 private string ToCommaSeparatedList( IList<string> packageNames )
		 {
			  StringBuilder sb = new StringBuilder();

			  foreach ( string str in packageNames )
			  {
					sb.Append( str );
					sb.Append( ", " );
			  }

			  string result = sb.ToString();
			  return result.Substring( 0, result.Length - 2 );
		 }

		 public class Packages : JaxRsServletHolderFactory
		 {
			  protected internal override void Configure( ServletHolder servletHolder, string packages )
			  {
					servletHolder.setInitParameter( PackagesResourceConfig.PROPERTY_PACKAGES, packages );
			  }
		 }

		 public class Classes : JaxRsServletHolderFactory
		 {
			  protected internal override void Configure( ServletHolder servletHolder, string classes )
			  {
					servletHolder.setInitParameter( ClassNamesResourceConfig.PROPERTY_CLASSNAMES, classes );
			  }
		 }
	}

}