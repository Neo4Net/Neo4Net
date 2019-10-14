using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery.kubernetes
{

	/// <summary>
	/// See <a href="https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.11/#servicelist-v1-core">ServiceList</a>
	/// </summary>
	public class ServiceList : KubernetesType
	{
		 private IList<Service> _items;

		 public ServiceList()
		 {
		 }

		 public virtual IList<Service> Items()
		 {
			  return _items;
		 }

		 public virtual IList<Service> Items
		 {
			 set
			 {
				  this._items = value;
			 }
		 }

		 public override T Handle<T>( Visitor<T> visitor )
		 {
			  return visitor.Visit( this );
		 }

		 public class Service
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ObjectMetadata MetadataConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ServiceSpec SpecConflict;

			  public Service()
			  {
			  }

			  public virtual ObjectMetadata Metadata()
			  {
					return MetadataConflict;
			  }

			  public virtual ServiceSpec Spec()
			  {
					return SpecConflict;
			  }

			  public virtual ObjectMetadata Metadata
			  {
				  set
				  {
						this.MetadataConflict = value;
				  }
			  }

			  public virtual ServiceSpec Spec
			  {
				  set
				  {
						this.SpecConflict = value;
				  }
			  }

			  public class ServiceSpec
			  {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
					internal IList<ServicePort> PortsConflict;

					public ServiceSpec()
					{
					}

					public virtual IList<ServicePort> Ports()
					{
						 return PortsConflict;
					}

					public virtual IList<ServicePort> Ports
					{
						set
						{
							 this.PortsConflict = value;
						}
					}

					public class ServicePort
					{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
						 internal string NameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
						 internal int PortConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
						 internal string ProtocolConflict;

						 public ServicePort()
						 {
						 }

						 public virtual string Name()
						 {
							  return NameConflict;
						 }

						 public virtual int Port()
						 {
							  return PortConflict;
						 }

						 public virtual string Protocol()
						 {
							  return ProtocolConflict;
						 }

						 public virtual string Name
						 {
							 set
							 {
								  this.NameConflict = value;
							 }
						 }

						 public virtual int Port
						 {
							 set
							 {
								  this.PortConflict = value;
							 }
						 }

						 public virtual string Protocol
						 {
							 set
							 {
								  this.ProtocolConflict = value;
							 }
						 }
					}
			  }
		 }
	}

}