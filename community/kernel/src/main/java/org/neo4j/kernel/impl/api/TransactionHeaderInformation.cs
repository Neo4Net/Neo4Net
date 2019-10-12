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
namespace Org.Neo4j.Kernel.Impl.Api
{
	public class TransactionHeaderInformation
	{
		 private readonly int _masterId;
		 private readonly int _authorId;
		 private readonly sbyte[] _additionalHeader;

		 public TransactionHeaderInformation( int masterId, int authorId, sbyte[] additionalHeader )
		 {
			  this._masterId = masterId;
			  this._authorId = authorId;
			  this._additionalHeader = additionalHeader;
		 }

		 public virtual int MasterId
		 {
			 get
			 {
				  return _masterId;
			 }
		 }

		 public virtual int AuthorId
		 {
			 get
			 {
				  return _authorId;
			 }
		 }

		 public virtual sbyte[] AdditionalHeader
		 {
			 get
			 {
				  return _additionalHeader;
			 }
		 }
	}

}