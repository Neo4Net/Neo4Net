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
namespace Neo4Net.Kernel.ha
{
	using Neo4Net.com;
	using RequestType = Neo4Net.com.RequestType;
	using Neo4Net.com;

	public class HaRequestType : RequestType
	{
		 private readonly TargetCaller _targetCaller;
		 private readonly ObjectSerializer _objectSerializer;
		 private readonly sbyte _id;
		 private readonly bool _unpack;

		 public HaRequestType( TargetCaller targetCaller, ObjectSerializer objectSerializer, sbyte id, bool unpack )
		 {
			  this._targetCaller = targetCaller;
			  this._objectSerializer = objectSerializer;
			  this._id = id;
			  this._unpack = unpack;
		 }

		 public virtual TargetCaller TargetCaller
		 {
			 get
			 {
				  return _targetCaller;
			 }
		 }

		 public virtual ObjectSerializer ObjectSerializer
		 {
			 get
			 {
				  return _objectSerializer;
			 }
		 }

		 public override sbyte Id()
		 {
			  return _id;
		 }

		 public override bool ResponseShouldBeUnpacked()
		 {
			  return _unpack;
		 }
	}

}