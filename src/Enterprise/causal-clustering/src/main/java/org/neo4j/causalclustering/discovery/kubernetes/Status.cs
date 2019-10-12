/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
	/// See <a href="https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.11/#status-v1-meta">Status</a>
	/// </summary>
	public class Status : KubernetesType
	{
		 private string _status;
		 private string _message;
		 private string _reason;
		 private int _code;

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual string StatusConflict()
		 {
			  return _status;
		 }

		 public virtual void SetStatus( string status )
		 {
			  this._status = status;
		 }

		 public virtual string Message()
		 {
			  return _message;
		 }

		 public virtual string Message
		 {
			 set
			 {
				  this._message = value;
			 }
		 }

		 public virtual string Reason()
		 {
			  return _reason;
		 }

		 public virtual string Reason
		 {
			 set
			 {
				  this._reason = value;
			 }
		 }

		 public virtual int Code()
		 {
			  return _code;
		 }

		 public virtual int Code
		 {
			 set
			 {
				  this._code = value;
			 }
		 }

		 public override T Handle<T>( Visitor<T> visitor )
		 {
			  return visitor.Visit( this );
		 }

		 public override string ToString()
		 {
			  return "Status{" + "status='" + _status + '\'' + ", message='" + _message + '\'' + ", reason='" + _reason + '\'' + ", code=" + _code + '}';
		 }
	}

}