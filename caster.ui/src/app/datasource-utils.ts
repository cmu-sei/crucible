/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon(R) and CERT(R) are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Observable, of, combineLatest, concat, defer, merge } from 'rxjs';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import {
  distinctUntilChanged,
  map,
  startWith,
  switchMap,
  tap,
} from 'rxjs/operators';
import { QueryList } from '@angular/core';

export class SimpleDataSource<T> extends DataSource<T> {
  constructor(private rows$: Observable<T[]>) {
    super();
  }
  connect(collectionViewer: CollectionViewer): Observable<T[]> {
    return this.rows$;
  }
  disconnect(collectionViewer: CollectionViewer): void {}
}

function defaultSort(a: any, b: any): number {
  //treat null === undefined for sorting
  a = a === undefined ? null : a;
  b = b === undefined ? null : b;

  if (a === b) {
    return 0;
  }
  if (a === null) {
    return -1;
  }
  if (b === null) {
    return 1;
  }

  //from this point on a & b can not be null or undefined.

  if (a > b) {
    return 1;
  } else if (a < b) {
    return -1;
  } else {
    return 0;
  }
}

type SortFn<U> = (a: U, b: U) => number;
interface PropertySortFns<U> {
  [prop: string]: SortFn<U>;
}

/** RxJS operator to map a material Sort object to a sort function */
function toSortFn<U>(
  sortFns: PropertySortFns<U> = {},
  useDefault = true
): (sort$: Observable<Sort>) => Observable<undefined | SortFn<U>> {
  return (sort$) =>
    sort$.pipe(
      map((sort) => {
        if (!sort.active || sort.direction === '') {
          return undefined;
        }

        let sortFn = sortFns[sort.active];
        if (!sortFn) {
          if (!useDefault) {
            throw new Error(`Unknown sort property [${sort.active}]`);
          }

          //By default assume sort.active is a property name, and sort using the default sort
          //  uses < and >.
          sortFn = (a: U, b: U) =>
            defaultSort((<any>a)[sort.active], (<any>b)[sort.active]);
        }

        return sort.direction === 'asc' ? sortFn : (a: U, b: U) => sortFn(b, a);
      })
    );
}

/** Creates an Observable stream of Sort objects from a MatSort component */
export function fromMatSort(sort: MatSort): Observable<Sort> {
  return concat(
    defer(() =>
      of({
        active: sort.active,
        direction: sort.direction,
      })
    ),
    sort.sortChange.asObservable()
  );
}

/** RxJs operator to sort an array based on an Observable of material Sort events **/
export function sortRows<U>(
  sort$: Observable<Sort>,
  sortFns: PropertySortFns<U> = {},
  useDefault = true
): (obs$: Observable<U[]>) => Observable<U[]> {
  return (rows$: Observable<U[]>) =>
    combineLatest(
      rows$,
      sort$.pipe(toSortFn(sortFns, useDefault)),
      (rows, sortFn) => {
        if (!sortFn) {
          return rows;
        }

        const copy = rows.slice();
        return copy.sort(sortFn);
      }
    );
}

/* TODO: handle ngIf'd MatPager controls.

function isEqualPageEvents(a: PageEvent, b: PageEvent) {
  return a.length === b.length
    && a.pageSize === b.pageSize
    && a.pageIndex === b.pageIndex;

}


export function fromMatPaginators(pagers: QueryList<MatPaginator>): Observable<PageEvent> {
  return pagers.changes.pipe(
    startWith(pagers),
    switchMap(_ =>
      merge(...pagers.map(p => fromMatPaginator(p))).pipe(
        distinctUntilChanged(isEqualPageEvents),
        tap(page => pagers.forEach(pager => {
          if (pager.pageIndex !== page.pageIndex) {
            pager.pageIndex = page.pageIndex;
          }
          if (pager.pageSize !== page.pageSize) {
            pager.pageSize = page.pageSize;
          }
          if (pager.length !== page.length) {
            pager.length = page.length;
          }
        }))
      )
    ),
    distinctUntilChanged(isEqualPageEvents),
  );
}
 */

/** Creates an Observable stream of PageEvent objects from a MatPaginator component */
export function fromMatPaginator(pager: MatPaginator): Observable<PageEvent> {
  return concat(
    defer(() =>
      of({
        pageIndex: pager.pageIndex,
        pageSize: pager.pageSize,
        length: pager.length,
      })
    ),
    pager.page.asObservable()
  );
}

/** RxJs operator to paginate an array based on an Observable of PageEvent objects **/
export function paginateRows<U>(
  page$: Observable<PageEvent>
): (obs$: Observable<U[]>) => Observable<U[]> {
  return (rows$: Observable<U[]>) =>
    combineLatest(rows$, page$, (rows, page) => {
      const startIndex = page.pageIndex * page.pageSize;
      const copy = rows.slice();
      return copy.splice(startIndex, page.pageSize);
    });
}
