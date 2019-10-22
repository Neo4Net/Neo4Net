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
	/// <summary>
	/// See <a href="https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.11/#objectmeta-v1-meta">ObjectMeta</a>
	/// </summary>
	public class ObjectMetadata
	{
		 private string _deletionTimestamp;
		 private string _name;

		 public ObjectMetadata()
		 {
		 }

		 public virtual string DeletionTimestamp()
		 {
			  return _deletionTimestamp;
		 }

		 public virtual string Name()
		 {
			  return _name;
		 }

		 public virtual string DeletionTimestamp
		 {
			 set
			 {
				  this._deletionTimestamp = value;
			 }
		 }

		 public virtual string Name
		 {
			 set
			 {
				  this._name = value;
			 }
		 }
	}

}