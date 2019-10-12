using System;

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


	using ObjectRepresentation = Org.Neo4j.Server.rest.repr.ObjectRepresentation;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;
	using RepresentationDispatcher = Org.Neo4j.Server.rest.repr.RepresentationDispatcher;
	using ValueRepresentation = Org.Neo4j.Server.rest.repr.ValueRepresentation;

	public class JmxAttributeRepresentation : ObjectRepresentation
	{

		 protected internal ObjectName ObjectName;
		 protected internal MBeanAttributeInfo AttrInfo;
		 protected internal MBeanServer JmxServer = ManagementFactory.PlatformMBeanServer;
		 private static readonly RepresentationDispatcher _representationDispatcher = new JmxAttributeRepresentationDispatcher();

		 public JmxAttributeRepresentation( ObjectName objectName, MBeanAttributeInfo attrInfo ) : base( "jmxAttribute" )
		 {
			  this.ObjectName = objectName;
			  this.AttrInfo = attrInfo;
		 }

		 [Mapping("name")]
		 public virtual ValueRepresentation Name
		 {
			 get
			 {
				  return ValueRepresentation.@string( AttrInfo.Name );
			 }
		 }

		 [Mapping("description")]
		 public virtual ValueRepresentation Description
		 {
			 get
			 {
				  return ValueRepresentation.@string( AttrInfo.Description );
			 }
		 }

		 [Mapping("type")]
		 public virtual ValueRepresentation Type
		 {
			 get
			 {
				  return ValueRepresentation.@string( AttrInfo.Type );
			 }
		 }

		 [Mapping("isReadable")]
		 public virtual ValueRepresentation Readable
		 {
			 get
			 {
				  return Bool( AttrInfo.Readable );
			 }
		 }

		 [Mapping("isWriteable")]
		 public virtual ValueRepresentation Writeable
		 {
			 get
			 {
				  return Bool( AttrInfo.Writable );
			 }
		 }

		 [Mapping("isIs")]
		 public virtual ValueRepresentation Is
		 {
			 get
			 {
				  return Bool( AttrInfo.Is );
			 }
		 }

		 private ValueRepresentation Bool( bool? value )
		 {
			  return ValueRepresentation.@string( value ? "true" : "false " );
		 }

		 [Mapping("value")]
		 public virtual Representation Value
		 {
			 get
			 {
				  try
				  {
						object value = JmxServer.getAttribute( ObjectName, AttrInfo.Name );
						return _representationDispatcher.dispatch( value, "" );
				  }
				  catch ( Exception )
				  {
						return ValueRepresentation.@string( "N/A" );
				  }
			 }
		 }

	}

}