﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Server.rest.management.repr
{

	using ListRepresentation = Org.Neo4j.Server.rest.repr.ListRepresentation;
	using ObjectRepresentation = Org.Neo4j.Server.rest.repr.ObjectRepresentation;
	using ValueRepresentation = Org.Neo4j.Server.rest.repr.ValueRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.ValueRepresentation.@string;

	public class JmxDomainRepresentation : ObjectRepresentation
	{

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal List<JmxMBeanRepresentation> BeansConflict = new List<JmxMBeanRepresentation>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal string DomainNameConflict;

		 public JmxDomainRepresentation( string name ) : base( "jmxDomain" )
		 {
			  this.DomainNameConflict = name;
		 }

		 [Mapping("domain")]
		 public virtual ValueRepresentation DomainName
		 {
			 get
			 {
				  return @string( this.DomainNameConflict );
			 }
		 }

		 [Mapping("beans")]
		 public virtual ListRepresentation Beans
		 {
			 get
			 {
				  return new ListRepresentation( "bean", BeansConflict );
			 }
		 }

		 public virtual void AddBean( ObjectName bean )
		 {
			  BeansConflict.Add( new JmxMBeanRepresentation( bean ) );
		 }
	}

}