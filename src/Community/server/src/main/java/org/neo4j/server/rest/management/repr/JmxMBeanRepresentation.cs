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
namespace Neo4Net.Server.rest.management.repr
{


	using Neo4Net.Helpers.Collections;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using ObjectRepresentation = Neo4Net.Server.rest.repr.ObjectRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

	public class JmxMBeanRepresentation : ObjectRepresentation
	{

		 protected internal ObjectName BeanName;
		 protected internal MBeanServer JmxServer = ManagementFactory.PlatformMBeanServer;

		 public JmxMBeanRepresentation( ObjectName beanInstance ) : base( "jmxBean" )
		 {
			  this.BeanName = beanInstance;
		 }

		 [Mapping("name")]
		 public virtual ValueRepresentation Name
		 {
			 get
			 {
				  return ValueRepresentation.@string( BeanName.CanonicalName );
			 }
		 }

		 [Mapping("url")]
		 public virtual ValueRepresentation Url
		 {
			 get
			 {
				  try
				  {
						string value = URLEncoder.encode( BeanName.ToString(), StandardCharsets.UTF_8.name() ).replace("%3A", "/");
						return ValueRepresentation.@string( value );
				  }
				  catch ( UnsupportedEncodingException e )
				  {
						throw new Exception( "Could not encode string as UTF-8", e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.ValueRepresentation getDescription() throws javax.management.IntrospectionException, javax.management.InstanceNotFoundException, javax.management.ReflectionException
		 [Mapping("description")]
		 public virtual ValueRepresentation Description
		 {
			 get
			 {
				  MBeanInfo beanInfo = JmxServer.getMBeanInfo( BeanName );
				  return ValueRepresentation.@string( beanInfo.Description );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.rest.repr.ListRepresentation getAttributes() throws javax.management.IntrospectionException, javax.management.InstanceNotFoundException, javax.management.ReflectionException
		 [Mapping("attributes")]
		 public virtual ListRepresentation Attributes
		 {
			 get
			 {
				  MBeanInfo beanInfo = JmxServer.getMBeanInfo( BeanName );
   
				  return new ListRepresentation( "jmxAttribute", new IterableWrapperAnonymousInnerClass( this, Arrays.asList( beanInfo.Attributes ) ) );
			 }
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, MBeanAttributeInfo>
		 {
			 private readonly JmxMBeanRepresentation _outerInstance;

			 public IterableWrapperAnonymousInnerClass( JmxMBeanRepresentation outerInstance, UnknownType asList ) : base( asList )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Representation underlyingObjectToObject( MBeanAttributeInfo attrInfo )
			 {
				  return new JmxAttributeRepresentation( _outerInstance.beanName, attrInfo );
			 }
		 }
	}

}