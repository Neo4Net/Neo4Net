using System;
using System.Collections.Generic;

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

namespace Neo4Net.Functions
{
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.function.ThrowingPredicate.throwingPredicate;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.function.ThrowingSupplier.throwingSupplier;

   /// <summary>
   /// Constructors for basic <seealso cref="Predicate"/> types
   /// </summary>
   public class Predicates
   {
      public static readonly System.Func<int, bool> AlwaysTrueInt = v => true;
      public static readonly System.Func<int, bool> AlwaysFalseInt = v => false;

      private const int DEFAULT_POLL_INTERVAL = 20;

      private Predicates()
      {
      }

      public static System.Predicate<T> AlwaysTrue<T>()
      {
         return x => true;
      }

      public static System.Predicate<T> AlwaysFalse<T>()
      {
         return x => false;
      }

      public static System.Predicate<T> NotNull<T>()
      {
         return Objects.nonNull;
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @SafeVarargs public static <T> System.Predicate<T> all(final System.Predicate<T>... predicates)
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      public static System.Predicate<T> All<T>(params System.Predicate<T>[] predicates)
      {
         return All(Arrays.asList(predicates));
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public static <T> System.Predicate<T> all(final Iterable<System.Predicate<T>> predicates)
      public static System.Predicate<T> All<T>(IEnumerable<System.Predicate<T>> predicates)
      {
         return item =>
         {
            foreach (Predicate<T> predicate in predicates)
            {
               if (!predicate.test(item))
               {
                  return false;
               }
            }
            return true;
         };
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @SafeVarargs public static <T> System.Predicate<T> any(final System.Predicate<T>... predicates)
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      public static System.Predicate<T> Any<T>(params System.Predicate<T>[] predicates)
      {
         return Any(Arrays.asList(predicates));
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public static <T> System.Predicate<T> any(final Iterable<System.Predicate<T>> predicates)
      public static System.Predicate<T> Any<T>(IEnumerable<System.Predicate<T>> predicates)
      {
         return item =>
         {
            foreach (Predicate<T> predicate in predicates)
            {
               if (predicate.Test(item))
               {
                  return true;
               }
            }
            return false;
         };
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: public static <T> java.util.function.Predicate<T> instanceOf(@Nonnull final Class clazz)
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      public static System.Predicate<T> InstanceOf<T>(Type clazz)
      {
            return typeof(T) == clazz; // clazz.isInstance;
      }

      
      public static System.Predicate<T> InstanceOfAny<T>(params Type[] classes)
      {
         return item =>
         {
            if (item != null)
            {
               foreach (Type clazz in classes)
               {
                  if (clazz.IsInstanceOfType(item))
                  {
                     return true;
                  }
               }
            }
            return false;
         };
      }

      public static System.Predicate<T> NoDuplicates<T>()
      {
         return new PredicateAnonymousInnerClass();
      }

      private class PredicateAnonymousInnerClass : System.Predicate<T>
      {
         private readonly ISet<T> visitedItems = new HashSet<T>();

         public override bool test(T item)
         {
            return visitedItems.add(item);
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <TYPE> TYPE await(System.Func<TYPE> supplier, System.Predicate<TYPE> predicate, long timeout, java.util.concurrent.TimeUnit timeoutUnit, long pollInterval, java.util.concurrent.TimeUnit pollUnit) throws java.util.concurrent.TimeoutException
      public static TYPE Await<TYPE>(System.Func<TYPE> supplier, System.Predicate<TYPE> predicate, long timeout, TimeUnit timeoutUnit, long pollInterval, TimeUnit pollUnit)
      {
         return AwaitEx(supplier.get, predicate.test, timeout, timeoutUnit, pollInterval, pollUnit);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <TYPE> TYPE await(System.Func<TYPE> supplier, System.Predicate<TYPE> predicate, long timeout, java.util.concurrent.TimeUnit timeoutUnit) throws java.util.concurrent.TimeoutException
      public static TYPE Await<TYPE>(System.Func<TYPE> supplier, System.Predicate<TYPE> predicate, long timeout, TimeUnit timeoutUnit)
      {
         return AwaitEx(throwingSupplier(supplier), throwingPredicate(predicate), timeout, timeoutUnit);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <TYPE, EXCEPTION extends Exception> TYPE awaitEx(ThrowingSupplier<TYPE,EXCEPTION> supplier, ThrowingPredicate<TYPE,EXCEPTION> predicate, long timeout, java.util.concurrent.TimeUnit timeoutUnit, long pollInterval, java.util.concurrent.TimeUnit pollUnit) throws java.util.concurrent.TimeoutException, EXCEPTION
      public static TYPE AwaitEx<TYPE, EXCEPTION>(ThrowingSupplier<TYPE, EXCEPTION> supplier, ThrowingPredicate<TYPE, EXCEPTION> predicate, long timeout, TimeUnit timeoutUnit, long pollInterval, TimeUnit pollUnit) where EXCEPTION : Exception
      {
         Suppliers.ThrowingCapturingSupplier<TYPE, EXCEPTION> composed = Suppliers.Compose(supplier, predicate);
         AwaitEx(composed, timeout, timeoutUnit, pollInterval, pollUnit);
         return composed.LastInput();
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <TYPE, EXCEPTION extends Exception> TYPE awaitEx(ThrowingSupplier<TYPE,? extends EXCEPTION> supplier, ThrowingPredicate<TYPE, ? extends EXCEPTION> predicate, long timeout, java.util.concurrent.TimeUnit timeoutUnit) throws java.util.concurrent.TimeoutException, EXCEPTION
      public static TYPE AwaitEx<TYPE, EXCEPTION, T1, T2>(ThrowingSupplier<T1> supplier, ThrowingPredicate<T2> predicate, long timeout, TimeUnit timeoutUnit) where EXCEPTION : Exception where T1 : EXCEPTION where T2 : EXCEPTION
      {
         Suppliers.ThrowingCapturingSupplier<TYPE, EXCEPTION> composed = Suppliers.Compose(supplier, predicate);
         AwaitEx(composed, timeout, timeoutUnit);
         return composed.LastInput();
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static void await(System.Func<boolean> condition, long timeout, java.util.concurrent.TimeUnit unit) throws java.util.concurrent.TimeoutException
      public static void Await(System.Func<bool> condition, long timeout, TimeUnit unit)
      {
         AwaitEx(condition.getAsBoolean, timeout, unit);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <EXCEPTION extends Exception> void awaitEx(ThrowingSupplier<bool,EXCEPTION> condition, long timeout, java.util.concurrent.TimeUnit unit) throws java.util.concurrent.TimeoutException, EXCEPTION
      public static void AwaitEx<EXCEPTION>(ThrowingSupplier<bool, EXCEPTION> condition, long timeout, TimeUnit unit) where EXCEPTION : Exception
      {
         AwaitEx(condition, timeout, unit, DEFAULT_POLL_INTERVAL, TimeUnit.MILLISECONDS);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static void await(System.Func<boolean> condition, long timeout, java.util.concurrent.TimeUnit timeoutUnit, long pollInterval, java.util.concurrent.TimeUnit pollUnit) throws java.util.concurrent.TimeoutException
      public static void Await(System.Func<bool> condition, long timeout, TimeUnit timeoutUnit, long pollInterval, TimeUnit pollUnit)
      {
         AwaitEx(condition.getAsBoolean, timeout, timeoutUnit, pollInterval, pollUnit);
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <EXCEPTION extends Exception> void awaitEx(ThrowingSupplier<bool,EXCEPTION> condition, long timeout, java.util.concurrent.TimeUnit unit, long pollInterval, java.util.concurrent.TimeUnit pollUnit) throws java.util.concurrent.TimeoutException, EXCEPTION
      public static void AwaitEx<EXCEPTION>(ThrowingSupplier<bool, EXCEPTION> condition, long timeout, TimeUnit unit, long pollInterval, TimeUnit pollUnit) where EXCEPTION : Exception
      {
         if (!TryAwaitEx(condition, timeout, unit, pollInterval, pollUnit))
         {
            throw new TimeoutException("Waited for " + timeout + " " + unit + ", but " + condition + " was not accepted.");
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <EXCEPTION extends Exception> boolean tryAwaitEx(ThrowingSupplier<bool,EXCEPTION> condition, long timeout, java.util.concurrent.TimeUnit timeoutUnit, long pollInterval, java.util.concurrent.TimeUnit pollUnit) throws EXCEPTION
      public static bool TryAwaitEx<EXCEPTION>(ThrowingSupplier<bool, EXCEPTION> condition, long timeout, TimeUnit timeoutUnit, long pollInterval, TimeUnit pollUnit) where EXCEPTION : Exception
      {
         return TryAwaitEx(condition, timeout, timeoutUnit, pollInterval, pollUnit, Clock.systemUTC());
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static <EXCEPTION extends Exception> boolean tryAwaitEx(ThrowingSupplier<bool,EXCEPTION> condition, long timeout, java.util.concurrent.TimeUnit timeoutUnit, long pollInterval, java.util.concurrent.TimeUnit pollUnit, java.time.Clock clock) throws EXCEPTION
      public static bool TryAwaitEx<EXCEPTION>(ThrowingSupplier<bool, EXCEPTION> condition, long timeout, TimeUnit timeoutUnit, long pollInterval, TimeUnit pollUnit, Clock clock) where EXCEPTION : Exception
      {
         long deadlineMillis = clock.millis() + timeoutUnit.toMillis(timeout);
         long pollIntervalNanos = pollUnit.toNanos(pollInterval);

         do
         {
            if (condition.Get())
            {
               return true;
            }
            LockSupport.parkNanos(pollIntervalNanos);
         } while (clock.millis() < deadlineMillis);
         return false;
      }

      public static void AwaitForever(System.Func<bool> condition, long checkInterval, TimeUnit unit)
      {
         long sleep = unit.toNanos(checkInterval);
         do
         {
            if (condition())
            {
               return;
            }
            LockSupport.parkNanos(sleep);
         } while (true);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @SafeVarargs public static <T> System.Predicate<T> in(final T... allowed)
      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      public static System.Predicate<T> In<T>(params T[] allowed)
      {
         return In(Arrays.asList(allowed));
      }

      public static System.Predicate<T> Not<T>(System.Predicate<T> predicate)
      {
         return t => !predicate(t);
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public static <T> System.Predicate<T> in(final Iterable<T> allowed)
      public static System.Predicate<T> In<T>(IEnumerable<T> allowed)
      {
         return item =>
         {
            foreach (T allow in allowed)
            {
               if (allow.Equals(item))
               {
                  return true;
               }
            }
            return false;
         };
      }

      public static System.Func<int, bool> Any(int[] values)
      {
         return v =>
         {
            foreach (int value in values)
            {
               if (v == value)
               {
                  return true;
               }
            }
            return false;
         };
      }
   }
}