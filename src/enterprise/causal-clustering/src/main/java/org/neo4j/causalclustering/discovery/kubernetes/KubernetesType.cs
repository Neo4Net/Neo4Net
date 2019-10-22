/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery.kubernetes
{
	using JsonSubTypes = org.codehaus.jackson.annotate.JsonSubTypes;
	using JsonTypeInfo = org.codehaus.jackson.annotate.JsonTypeInfo;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JsonTypeInfo(use = JsonTypeInfo.Id.NAME, property = "kind") @JsonSubTypes({ @JsonSubTypes.Type(value = ServiceList.class, name = "ServiceList"), @JsonSubTypes.Type(value = Status.class, name = "Status") }) public abstract class KubernetesType
	public abstract class KubernetesType
	{
		 private string _kind;

		 public virtual string Kind()
		 {
			  return _kind;
		 }

		 public virtual string Kind
		 {
			 set
			 {
				  this._kind = value;
			 }
		 }

		 public abstract T handle<T>( Visitor<T> visitor );

		 public interface Visitor<T>
		 {
			  T Visit( Status status );

			  T Visit( ServiceList serviceList );
		 }
	}

}