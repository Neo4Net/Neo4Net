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
	using ClassNamesResourceConfig = com.sun.jersey.api.core.ClassNamesResourceConfig;
	using ResourceConfig = com.sun.jersey.api.core.ResourceConfig;
	using WebApplication = com.sun.jersey.spi.container.WebApplication;
	using ServletContainer = com.sun.jersey.spi.container.servlet.ServletContainer;
	using WebConfig = com.sun.jersey.spi.container.servlet.WebConfig;


	using Neo4Net.Server.database;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("serial") public class NeoServletContainer extends com.sun.jersey.spi.container.servlet.ServletContainer
	public class NeoServletContainer : ServletContainer
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Collection<Neo4Net.server.database.InjectableProvider<?>> injectables;
		 private readonly ICollection<InjectableProvider<object>> _injectables;

		 public NeoServletContainer<T1>( ICollection<T1> injectables )
		 {
			  this._injectables = injectables;
		 }

		 protected internal override void Configure( WebConfig wc, ResourceConfig rc, WebApplication wa )
		 {
			  base.Configure( wc, rc, wa );

			  ISet<object> singletons = rc.Singletons;
			  singletons.addAll( _injectables );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected com.sun.jersey.api.core.ResourceConfig getDefaultResourceConfig(java.util.Map<String, Object> props, com.sun.jersey.spi.container.servlet.WebConfig wc) throws javax.servlet.ServletException
		 protected internal override ResourceConfig GetDefaultResourceConfig( IDictionary<string, object> props, WebConfig wc )
		 {
			  object classNames = props[ClassNamesResourceConfig.PROPERTY_CLASSNAMES];
			  if ( classNames != null )
			  {
					return new ClassNamesResourceConfig( props );
			  }

			  return base.GetDefaultResourceConfig( props, wc );
		 }
	}

}