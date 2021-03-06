﻿/*
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
namespace Org.Neo4j.Consistency.checking.cache
{
	public interface CacheSlots
	{
	}

	public static class CacheSlots_Fields
	{
		 public const int LABELS_SLOT_SIZE = 40;
		 public const int ID_SLOT_SIZE = 40;
	}

	 public interface CacheSlots_NodeLabel
	 {
	 }

	 public static class CacheSlots_NodeLabel_Fields
	 {
		  public const int SLOT_LABEL_FIELD = 0;
		  public const int SLOT_IN_USE = 1;
	 }

	 public interface CacheSlots_NextRelationship
	 {
	 }

	 public static class CacheSlots_NextRelationship_Fields
	 {
		  public const int SLOT_RELATIONSHIP_ID = 0;
		  public const int SLOT_FIRST_IN_SOURCE = 1;
		  public const int SLOT_FIRST_IN_TARGET = 2;
	 }

	 public interface CacheSlots_RelationshipLink
	 {
	 }

	 public static class CacheSlots_RelationshipLink_Fields
	 {
		  public const int SLOT_RELATIONSHIP_ID = 0;
		  public const int SLOT_REFERENCE = 1;
		  public const int SLOT_SOURCE_OR_TARGET = 2;
		  public const int SLOT_PREV_OR_NEXT = 3;
		  public const int SLOT_IN_USE = 4;
		  public const long SOURCE = 0;
		  public const long TARGET = -1;
		  public const long PREV = 0;
		  public const long NEXT = -1;
	 }

}